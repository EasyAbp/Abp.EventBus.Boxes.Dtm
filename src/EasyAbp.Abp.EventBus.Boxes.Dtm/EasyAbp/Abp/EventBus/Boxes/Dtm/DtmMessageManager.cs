﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dtmgrpc;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class DtmMessageManager : IDtmMessageManager, IScopedDependency
{
    /// <summary>
    /// DTM message for non-transactional DbContexts.
    /// </summary>
    protected MsgGrpc DefaultDtmMessage { get; set; }
    
    /// <summary>
    /// Distributed events of non-transactional DbContext.
    /// </summary>
    protected List<OutgoingEventInfo> DefaultEvents { get; set; }
    
    /// <summary>
    /// DTM message for each transactional DbContext.
    /// </summary>
    protected ConcurrentDictionary<DbTransaction, MsgGrpc> TransMessages { get; set; } = new();
    
    /// <summary>
    /// Distributed events of each transactional DbContext.
    /// </summary>
    protected ConcurrentDictionary<DbTransaction, List<OutgoingEventInfo>> TransEvents { get; set; } = new();

    protected ICurrentTenant CurrentTenant { get; }
    
    protected IDtmMsgGidProvider GidProvider { get; }
    
    protected IDtmTransFactory DtmTransFactory { get; }
    
    protected IEventInfosSerializer EventInfosSerializer { get; }
    
    protected IConnectionStringHasher ConnectionStringHasher { get; }

    protected DtmOutboxOptions DtmOutboxOptions { get; }

    public DtmMessageManager(
        ICurrentTenant currentTenant,
        IDtmMsgGidProvider gidProvider,
        IDtmTransFactory dtmTransFactory,
        IEventInfosSerializer eventInfosSerializer,
        IConnectionStringHasher connectionStringHasher,
        IOptions<DtmOutboxOptions> dtmOutboxOptions)
    {
        CurrentTenant = currentTenant;
        GidProvider = gidProvider;
        DtmTransFactory = dtmTransFactory;
        EventInfosSerializer = eventInfosSerializer;
        ConnectionStringHasher = connectionStringHasher;
        DtmOutboxOptions = dtmOutboxOptions.Value;
    }
    
    public virtual async Task AddEventAsync(object dbContext, DbTransaction dbTransaction, OutgoingEventInfo eventInfo)
    {
        if (dbTransaction is null)
        {
            DefaultEvents.Add(eventInfo);
        }
        else
        {
            var events = TransEvents.GetOrAdd(dbTransaction, () => new List<OutgoingEventInfo>());

            eventInfo.SetProperty(OutgoingEventInfoProperties.ConnectionStringName,
                ConnectionStringNameAttribute.GetConnStringName(dbContext.GetType()));

            eventInfo.SetProperty(OutgoingEventInfoProperties.TenantId, CurrentTenant.Id);

            eventInfo.SetProperty(OutgoingEventInfoProperties.HashedConnectionString,
                await ConnectionStringHasher.HashAsync(dbTransaction.Connection.ConnectionString));
            
            events.Add(eventInfo);
        }
    }

    public virtual async Task PrepareAsync(CancellationToken cancellationToken = default)
    {
        var defaultMsg = GetOrCreateDtmMessage(null);
        AddEventsPublishingAction(defaultMsg, DefaultEvents);

        foreach (var (dbTransaction, eventInfos) in TransEvents)
        {
            var msg = GetOrCreateDtmMessage(dbTransaction);
            AddEventsPublishingAction(msg, eventInfos);
        }
        
        // Don't need to prepare for the default (non-transactional) msg.
        
        foreach (var (dbTransaction, dtmMessage) in TransMessages)
        {
            await dtmMessage.Prepare(GenerateQueryPreparedAddress(TransEvents[dbTransaction]), cancellationToken);
        }
    }

    public virtual async Task SubmitAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (_, dtmMessage) in TransMessages)
        {
            await dtmMessage.Submit(cancellationToken);
        }

        await DefaultDtmMessage.Submit(cancellationToken);
    }

    protected virtual MsgGrpc GetOrCreateDtmMessage([CanBeNull] DbTransaction dbTransaction)
    {
        if (dbTransaction is null)
        {
            DefaultDtmMessage ??= DtmTransFactory.NewMsgGrpc(GidProvider.Create());
            
            return DefaultDtmMessage;
        }
        
        TransMessages.GetOrAdd(dbTransaction, _ => DtmTransFactory.NewMsgGrpc(GidProvider.Create()));

        return TransMessages[dbTransaction];
    }
    
    protected virtual void AddEventsPublishingAction(MsgGrpc msg, List<OutgoingEventInfo> eventInfos)
    {
        msg.Add(DtmOutboxOptions.GetPublishEventsAddress(), new DtmMsgPublishEventsRequest
        {
            ActionApiToken = DtmOutboxOptions.ActionApiToken,
            OutgoingEventInfoListToByteString = EventInfosSerializer.Serialize(eventInfos)
        });
    }
    
    protected virtual string GenerateQueryPreparedAddress(IEnumerable<OutgoingEventInfo> events)
    {
        var baseUrl = DtmOutboxOptions.GetQueryPreparedAddress();

        var firstEvent = events.First();
        
        var extraParams =
            $"Info.ConnectionStringName={firstEvent.GetProperty<string>(OutgoingEventInfoProperties.ConnectionStringName)}&Info.TenantId={firstEvent.GetProperty<Guid?>(OutgoingEventInfoProperties.ConnectionStringName)}&Info.HashedConnectionString={firstEvent.GetProperty<string>(OutgoingEventInfoProperties.HashedConnectionString)}";
        
        return baseUrl.Contains('?') ? $"{baseUrl.EnsureEndsWith('&')}{extraParams}" : $"{baseUrl}?{extraParams}";
    }
}