﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.DistributedEvents;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;

public class DtmDbContextEventOutbox<TDbContext> : IMongoDbContextEventOutbox<TDbContext> where TDbContext : IHasEventOutbox
{
    protected IMongoDbContextProvider<TDbContext> DbContextProvider { get; }
    protected DtmOutboxOptions DtmOutboxOptions { get; }
    protected IDtmMessageManager DtmMessageManager { get; }

    public DtmDbContextEventOutbox(
        IMongoDbContextProvider<TDbContext> dbContextProvider,
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

        await DtmMessageManager.AddEventAsync(
            dbContext,
            ConnectionStringNameAttribute.GetConnStringName<TDbContext>(),
            dbContext.SessionHandle,
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