using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class FakeDtmMessageManager : IDtmMessageManager, ITransientDependency
{
    protected ICurrentTenant CurrentTenant { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected IDtmMsgGidProvider GidProvider { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected IConnectionStringHasher ConnectionStringHasher { get; }
    protected IConnectionStringResolver ConnectionStringResolver { get; }

    public FakeDtmMessageManager(
        ICurrentTenant currentTenant,
        IServiceProvider serviceProvider,
        IDtmMsgGidProvider gidProvider,
        IUnitOfWorkManager unitOfWorkManager,
        IConnectionStringHasher connectionStringHasher,
        IConnectionStringResolver connectionStringResolver)
    {
        CurrentTenant = currentTenant;
        ServiceProvider = serviceProvider;
        GidProvider = gidProvider;
        UnitOfWorkManager = unitOfWorkManager;
        ConnectionStringHasher = connectionStringHasher;
        ConnectionStringResolver = connectionStringResolver;
    }

    public virtual async Task AddEventAsync(DtmOutboxEventBag eventBag, object dbContext, string connectionString,
        object transObj, OutgoingEventInfo eventInfo)
    {
        var dbContextType = dbContext.GetType();
        var hashedConnectionString = await ConnectionStringHasher.HashAsync(connectionString);

        var model = GetOrCreateDtmMessageInfoModel(eventBag, transObj, dbContextType, hashedConnectionString);

        model.EventInfos.Add(eventInfo);
    }

    public async Task InsertBarriersAndPrepareAsync(DtmOutboxEventBag eventBag,
        CancellationToken cancellationToken = default)
    {
        // await AddEventsPublishingActionAsync(eventBag);

        // WARNING:
        // the defaultMsg (with non-transactional events) will not invoke DTM's Prepare and not create a barrier.
        // That means when the DTM crashes, non-transactional events will never publish.
        // To avoid this problem, please KEEP USING TRANSACTION if you need write-operations.

        // await PrepareTransMessagesAsync(eventBag, cancellationToken);
        
        await InsertTransMessagesBarriersAsync(eventBag, cancellationToken);
    }

    public virtual async Task SubmitAsync(DtmOutboxEventBag eventBag, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
    
    protected virtual async Task InsertTransMessagesBarriersAsync(DtmOutboxEventBag eventBag,
        CancellationToken cancellationToken = default)
    {
        foreach (var model in eventBag.TransMessages.Values)
        {
            var barrierManagers = ServiceProvider.GetServices<IDtmMsgBarrierManager>();

            var databaseApi = await GetDatabaseApiAsync(model.DbConnectionLookupInfo.DbContextType);

            var inserted = false;

            foreach (var barrierManager in barrierManagers)
            {
                if (await barrierManager.TryInvokeEnsureInsertBarrierAsync(databaseApi, model.Gid, cancellationToken))
                {
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
            {
                throw new AbpException(
                    $"No match DTM message barrier manager to {model.DbConnectionLookupInfo.DbContextType.Name}.");
            }
        }
    }
    
    protected virtual async Task<IDatabaseApi> GetDatabaseApiAsync(Type targetDbContextType)
    {
        var connectionString = await ConnectionStringResolver.ResolveAsync(targetDbContextType);

        var databaseApiKey = $"{targetDbContextType.FullName}_{connectionString}";

        var databaseApi = UnitOfWorkManager.Current.FindDatabaseApi(databaseApiKey);

        Check.NotNull(databaseApi, nameof(databaseApi));

        return databaseApi;
    }
    
    protected virtual IDtmMessageInfoModel GetOrCreateDtmMessageInfoModel(DtmOutboxEventBag eventBag,
        [CanBeNull] object transObj, Type dbContextType, string hashedConnectionString)
    {
        if (transObj is null)
        {
            return eventBag.DefaultMessage ?? (eventBag.DefaultMessage =
                CreateDtmMessageInfoModel(dbContextType, hashedConnectionString));
        }

        return eventBag.TransMessages.GetOrAdd(transObj,
            _ => CreateDtmMessageInfoModel(dbContextType, hashedConnectionString));
    }

    protected virtual IDtmMessageInfoModel CreateDtmMessageInfoModel(Type dbContextType, string hashedConnectionString)
    {
        var gid = GidProvider.Create();

        return new FakeDtmMessageInfoModel(gid, null,
            new DbConnectionLookupInfoModel(dbContextType, CurrentTenant.Id, hashedConnectionString));
    }
}