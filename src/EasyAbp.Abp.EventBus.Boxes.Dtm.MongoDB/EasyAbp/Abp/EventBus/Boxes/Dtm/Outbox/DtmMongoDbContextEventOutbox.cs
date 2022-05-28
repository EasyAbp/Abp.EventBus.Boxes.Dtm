using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.DistributedEvents;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;

public class DtmMongoDbContextEventOutbox<TDbContext> : IDtmMongoDbContextEventOutbox<TDbContext>
    where TDbContext : IAbpMongoDbContext
{
    protected IMongoDbContextProvider<TDbContext> DbContextProvider { get; }

    protected AsyncLocalDtmOutboxEventBag AsyncLocalEventBag { get; }

    protected AbpDtmEventBoxesOptions AbpDtmEventBoxesOptions { get; }

    protected IDtmMessageManager DtmMessageManager { get; }

    public DtmMongoDbContextEventOutbox(
        IMongoDbContextProvider<TDbContext> dbContextProvider,
        AsyncLocalDtmOutboxEventBag asyncLocalEventBag,
        IOptions<AbpDtmEventBoxesOptions> dtmOutboxOptions,
        IDtmMessageManager dtmMessageManager)
    {
        DbContextProvider = dbContextProvider;
        AsyncLocalEventBag = asyncLocalEventBag;
        AbpDtmEventBoxesOptions = dtmOutboxOptions.Value;
        DtmMessageManager = dtmMessageManager;
    }

    public virtual async Task EnqueueAsync(OutgoingEventInfo outgoingEvent)
    {
        var dbContext = await DbContextProvider.GetDbContextAsync();

        await DtmMessageManager.AddEventAsync(
            AsyncLocalEventBag.GetOrCreate(),
            dbContext,
            ConnectionStringNameAttribute.GetConnStringName<TDbContext>(),
            dbContext.SessionHandle,
            outgoingEvent);
    }

    public virtual Task<List<OutgoingEventInfo>> GetWaitingEventsAsync(int maxCount,
        CancellationToken cancellationToken = new())
    {
        throw new NotSupportedException();
    }

    public virtual Task DeleteAsync(Guid id)
    {
        throw new NotSupportedException();
    }

    public async Task DeleteManyAsync(IEnumerable<Guid> ids)
    {
        throw new NotSupportedException();
    }
}