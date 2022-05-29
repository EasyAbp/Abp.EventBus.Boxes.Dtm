using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dtmgrpc;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Models;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class DtmMessageManager : IDtmMessageManager, ITransientDependency
{
    protected ICurrentTenant CurrentTenant { get; }
    
    protected IDtmMsgGidProvider GidProvider { get; }
    
    protected IDtmTransFactory DtmTransFactory { get; }
    
    protected IServiceProvider ServiceProvider { get; }
    
    protected IDtmMsgGidProvider DtmMsgGidProvider { get; }
    
    protected IUnitOfWorkManager UnitOfWorkManager { get; }

    protected IEventInfosSerializer EventInfosSerializer { get; }
    
    protected IConnectionStringHasher ConnectionStringHasher { get; }
    
    protected IConnectionStringResolver ConnectionStringResolver { get; }

    protected AbpDtmEventBoxesOptions AbpDtmEventBoxesOptions { get; }

    public DtmMessageManager(
        ICurrentTenant currentTenant,
        IDtmMsgGidProvider gidProvider,
        IDtmTransFactory dtmTransFactory,
        IServiceProvider serviceProvider,
        IDtmMsgGidProvider dtmMsgGidProvider,
        IUnitOfWorkManager unitOfWorkManager,
        IEventInfosSerializer eventInfosSerializer,
        IConnectionStringHasher connectionStringHasher,
        IConnectionStringResolver connectionStringResolver,
        IOptions<AbpDtmEventBoxesOptions> dtmOutboxOptions)
    {
        CurrentTenant = currentTenant;
        GidProvider = gidProvider;
        DtmTransFactory = dtmTransFactory;
        ServiceProvider = serviceProvider;
        DtmMsgGidProvider = dtmMsgGidProvider;
        UnitOfWorkManager = unitOfWorkManager;
        EventInfosSerializer = eventInfosSerializer;
        ConnectionStringHasher = connectionStringHasher;
        ConnectionStringResolver = connectionStringResolver;
        AbpDtmEventBoxesOptions = dtmOutboxOptions.Value;
    }

    public virtual async Task AddEventAsync(DtmOutboxEventBag eventBag, object dbContext, string connectionString,
        object transObj, OutgoingEventInfo eventInfo)
    {
        var dbContextType = dbContext.GetType();
        var hashedConnectionString = await ConnectionStringHasher.HashAsync(connectionString);

        var model = GetOrCreateDtmMessageInfoModel(eventBag, transObj, dbContextType, hashedConnectionString);

        model.EventInfos.Add(eventInfo);
    }

    public virtual async Task InsertBarriersAndPrepareAsync(DtmOutboxEventBag eventBag,
        CancellationToken cancellationToken = default)
    {
        await AddEventsPublishingActionAsync(eventBag);

        // WARNING:
        // the defaultMsg (with non-transactional events) will not invoke DTM's Prepare and not create a barrier.
        // That means when the DTM crashes, non-transactional events will never publish.
        // To avoid this problem, please KEEP USING TRANSACTION if you need write-operations.
        
        await PrepareTransMessagesAsync(eventBag);

        await InsertTransMessagesBarriersAsync(eventBag);
    }

    public virtual async Task SubmitAsync(DtmOutboxEventBag eventBag, CancellationToken cancellationToken = default)
    {
        if (eventBag.DefaultMessage is not null)
        {
            await eventBag.DefaultMessage.DtmMessage.Submit(cancellationToken);
        }

        foreach (var model in eventBag.TransMessages.Values)
        {
            await model.DtmMessage.Submit(cancellationToken);
        }
    }

    protected virtual DtmMessageInfoModel GetOrCreateDtmMessageInfoModel(DtmOutboxEventBag eventBag,
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

    protected virtual DtmMessageInfoModel CreateDtmMessageInfoModel(Type dbContextType, string hashedConnectionString)
    {
        var gid = GidProvider.Create();

        return new DtmMessageInfoModel(gid, DtmTransFactory.NewMsgGrpc(gid),
            new DbConnectionLookupInfoModel(dbContextType, CurrentTenant.Id, hashedConnectionString));
    }

    protected virtual Task AddEventsPublishingActionAsync(DtmOutboxEventBag eventBag)
    {
        eventBag.DefaultMessage?.AddEventsPublishingAction(AbpDtmEventBoxesOptions, EventInfosSerializer);

        foreach (var model in eventBag.TransMessages.Values)
        {
            model.AddEventsPublishingAction(AbpDtmEventBoxesOptions, EventInfosSerializer);
        }

        return Task.CompletedTask;
    }

    protected virtual async Task PrepareTransMessagesAsync(DtmOutboxEventBag eventBag)
    {
        foreach (var model in eventBag.TransMessages.Values)
        {
            await model.DtmMessage.Prepare(GenerateQueryPreparedAddress(model));
        }
    }

    protected virtual async Task InsertTransMessagesBarriersAsync(DtmOutboxEventBag eventBag)
    {
        foreach (var model in eventBag.TransMessages.Values)
        {
            var barrierManagers = ServiceProvider.GetServices<IDtmMsgBarrierManager>();

            foreach (var barrierManager in barrierManagers)
            {
                await barrierManager.TryInvokeInsertBarrierAsync(
                    await GetDatabaseApiAsync(model.DbConnectionLookupInfo.DbContextType),
                    DtmMsgGidProvider.Create()
                );
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

    protected virtual string GenerateQueryPreparedAddress(DtmMessageInfoModel model)
    {
        var baseUrl = AbpDtmEventBoxesOptions.GetQueryPreparedAddress();

        var extraParams =
            $"Info.DbContext={model.DbConnectionLookupInfo.DbContextType.FullName}&Info.TenantId={model.DbConnectionLookupInfo.TenantId}&Info.HashedConnectionString={model.DbConnectionLookupInfo.HashedConnectionString}";
        
        return baseUrl.Contains('?') ? $"{baseUrl.EnsureEndsWith('&')}{extraParams}" : $"{baseUrl}?{extraParams}";
    }
}