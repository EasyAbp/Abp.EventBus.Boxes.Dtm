using System;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Models;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class FakeDtmMessageManager : IDtmMessageManager, ITransientDependency
{
    protected IServiceProvider ServiceProvider { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected IConnectionStringResolver ConnectionStringResolver { get; }

    public FakeDtmMessageManager(
        IServiceProvider serviceProvider,
        IUnitOfWorkManager unitOfWorkManager,
        IConnectionStringResolver connectionStringResolver)
    {
        ServiceProvider = serviceProvider;
        UnitOfWorkManager = unitOfWorkManager;
        ConnectionStringResolver = connectionStringResolver;
    }

    public virtual async Task AddEventAsync(DtmOutboxEventBag eventBag, object dbContext, string connectionString,
        object transObj, OutgoingEventInfo eventInfo)
    {
        await Task.CompletedTask;
    }

    public async Task InsertBarriersAndPrepareAsync(DtmOutboxEventBag eventBag,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    public virtual async Task SubmitAsync(DtmOutboxEventBag eventBag, CancellationToken cancellationToken = default)
    {
        // await AddEventsPublishingActionAsync(eventBag);

        // WARNING:
        // the defaultMsg (with non-transactional events) will not invoke DTM's Prepare and not create a barrier.
        // That means when the DTM crashes, non-transactional events will never publish.
        // To avoid this problem, please KEEP USING TRANSACTION if you need write-operations.

        // await PrepareTransMessagesAsync(eventBag, cancellationToken);
        
        await InsertTransMessagesBarriersAsync(eventBag);
    }
    
    protected virtual async Task InsertTransMessagesBarriersAsync(DtmOutboxEventBag eventBag)
    {
        foreach (var model in eventBag.TransMessages.Values)
        {
            var barrierManagers = ServiceProvider.GetServices<IDtmMsgBarrierManager>();

            var databaseApi = await GetDatabaseApiAsync(model.DbConnectionLookupInfo.DbContextType);

            var inserted = false;

            foreach (var barrierManager in barrierManagers)
            {
                if (await barrierManager.TryInvokeEnsureInsertBarrierAsync(databaseApi, model.Gid))
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
}