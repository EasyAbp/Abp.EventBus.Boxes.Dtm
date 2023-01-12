using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.Abp.EventBus.Distributed.Dtm;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Inbox;

public class DtmMongoDbContextEventInbox<TDbContext> : IDtmMongoDbContextEventInbox<TDbContext>
    where TDbContext : IAbpMongoDbContext
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected IDistributedEventBus DistributedEventBus { get; }
    protected IMongoDbContextProvider<TDbContext> DbContextProvider { get; }
    protected IDtmInboxBarrierManager<IAbpMongoDbContext> DtmInboxBarrierManager { get; }

    public DtmMongoDbContextEventInbox(
        IUnitOfWorkManager unitOfWorkManager,
        IDistributedEventBus distributedEventBus,
        IMongoDbContextProvider<TDbContext> dbContextProvider,
        IDtmInboxBarrierManager<IAbpMongoDbContext> dtmInboxBarrierManager)
    {
        UnitOfWorkManager = unitOfWorkManager;
        DistributedEventBus = distributedEventBus;
        DbContextProvider = dbContextProvider;
        DtmInboxBarrierManager = dtmInboxBarrierManager;
    }
    
    public virtual async Task EnqueueAsync(IncomingEventInfo incomingEvent)
    {
        using var uow = UnitOfWorkManager.Begin(isTransactional: true, requiresNew: true);
        
        var dbContext = await DbContextProvider.GetDbContextAsync();

        await DtmInboxBarrierManager.EnsureInsertBarrierAsync(dbContext, incomingEvent.MessageId);

        await DistributedEventBus
            .AsSupportsEventBoxes()
            .ProcessFromInboxAsync(incomingEvent, null);

        await uow.CompleteAsync();
    }

    public virtual Task<List<IncomingEventInfo>> GetWaitingEventsAsync(int maxCount,
        CancellationToken cancellationToken = new())
    {
        throw new NotSupportedException();
    }

    public virtual Task MarkAsProcessedAsync(Guid id)
    {
        throw new NotSupportedException();
    }

    [UnitOfWork(false)]
    public virtual async Task<bool> ExistsByMessageIdAsync(string messageId)
    {
        var dbContext = await DbContextProvider.GetDbContextAsync();

        return await DtmInboxBarrierManager.ExistBarrierAsync(dbContext, messageId);
    }

    public virtual Task DeleteOldEventsAsync()
    {
        throw new NotSupportedException();
    }
}