using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;

public class DtmDbContextEventOutbox<TDbContext> : IDbContextEventOutbox<TDbContext> where TDbContext : IHasEventOutbox
{
    protected IDbContextProvider<TDbContext> DbContextProvider { get; }
    protected IDtmMessageManager DtmMessageManager { get; }

    public DtmDbContextEventOutbox(
        IDbContextProvider<TDbContext> dbContextProvider,
        IDtmMessageManager dtmMessageManager)
    {
        DbContextProvider = dbContextProvider;
        DtmMessageManager = dtmMessageManager;
    }
    
    public virtual async Task EnqueueAsync(OutgoingEventInfo outgoingEvent)
    {
        var dbContext = await DbContextProvider.GetDbContextAsync();

        await DtmMessageManager.AddEventAsync(
            typeof(TDbContext),
            dbContext.Database.GetConnectionString() ?? throw new InvalidOperationException(),
            dbContext.Database.CurrentTransaction?.GetDbTransaction(),
            outgoingEvent);
    }

    public virtual Task<List<OutgoingEventInfo>> GetWaitingEventsAsync(int maxCount, CancellationToken cancellationToken = new())
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