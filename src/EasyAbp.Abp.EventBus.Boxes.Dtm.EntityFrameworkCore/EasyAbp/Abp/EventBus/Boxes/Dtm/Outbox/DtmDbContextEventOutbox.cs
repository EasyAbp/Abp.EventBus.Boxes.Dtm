using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;

public class DtmDbContextEventOutbox<TDbContext> : IDbContextEventOutbox<TDbContext> where TDbContext : IHasEventOutbox
{
    protected IDbContextProvider<TDbContext> DbContextProvider { get; }
    protected DtmOutboxOptions DtmOutboxOptions { get; }
    protected IDtmMessageManager DtmMessageManager { get; }

    public DtmDbContextEventOutbox(
        IDbContextProvider<TDbContext> dbContextProvider,
        IOptions<DtmOutboxOptions> dtmOutboxOptions,
        IDtmMessageManager dtmMessageManager)
    {
        DbContextProvider = dbContextProvider;
        DtmOutboxOptions = dtmOutboxOptions.Value;
        DtmMessageManager = dtmMessageManager;
    }
    
    public virtual async Task EnqueueAsync(OutgoingEventInfo outgoingEvent)
    {
        var dbContext = await DbContextProvider.GetDbContextAsync();
        var dbTransaction = dbContext.Database.CurrentTransaction?.GetDbTransaction();

        await DtmMessageManager.AddEventAsync(dbContext, dbTransaction, outgoingEvent);
    }

    public virtual Task<List<OutgoingEventInfo>> GetWaitingEventsAsync(int maxCount, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    public virtual Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteManyAsync(IEnumerable<Guid> ids)
    {
        throw new NotImplementedException();
    }
}