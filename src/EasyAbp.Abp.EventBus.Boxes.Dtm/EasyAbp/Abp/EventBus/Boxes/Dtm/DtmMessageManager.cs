using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dtmgrpc;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Models;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class DtmMessageManager : IDtmMessageManager, IScopedDependency
{
    /// <summary>
    /// DTM message for non-transactional distributed events.
    /// </summary>
    protected DtmMessageInfoModel DefaultDtmMessage { get; set; }
    
    /// <summary>
    /// Non-transactional distributed events.
    /// </summary>
    protected DtmMessageEventList DefaultEvents { get; set; }
    
    /// <summary>
    /// DTM message for each transaction.
    /// </summary>
    protected ConcurrentDictionary<object, DtmMessageInfoModel> TransMessages { get; set; } = new();
    
    /// <summary>
    /// Distributed events of each transaction.
    /// </summary>
    protected ConcurrentDictionary<object, DtmMessageEventList> TransEvents { get; set; } = new();

    protected ICurrentTenant CurrentTenant { get; }
    
    protected IDtmMsgGidProvider GidProvider { get; }
    
    protected IDtmTransFactory DtmTransFactory { get; }
    
    protected IBranchBarrierFactory BranchBarrierFactory { get; }

    protected IEventInfosSerializer EventInfosSerializer { get; }
    
    protected IConnectionStringHasher ConnectionStringHasher { get; }

    protected DtmOutboxOptions DtmOutboxOptions { get; }

    public DtmMessageManager(
        ICurrentTenant currentTenant,
        IDtmMsgGidProvider gidProvider,
        IDtmTransFactory dtmTransFactory,
        IBranchBarrierFactory branchBarrierFactory,
        IEventInfosSerializer eventInfosSerializer,
        IConnectionStringHasher connectionStringHasher,
        IOptions<DtmOutboxOptions> dtmOutboxOptions)
    {
        CurrentTenant = currentTenant;
        GidProvider = gidProvider;
        DtmTransFactory = dtmTransFactory;
        BranchBarrierFactory = branchBarrierFactory;
        EventInfosSerializer = eventInfosSerializer;
        ConnectionStringHasher = connectionStringHasher;
        DtmOutboxOptions = dtmOutboxOptions.Value;
    }

    public virtual async Task AddEventAsync(Type dbContextType, string connectionString, object transObj,
        OutgoingEventInfo eventInfo)
    {
        var hashedConnectionString = await ConnectionStringHasher.HashAsync(connectionString);

        if (transObj is null)
        {
            DefaultEvents ??=
                new DtmMessageEventList(new DbConnectionLookupInfoModel(dbContextType, CurrentTenant.Id,
                    hashedConnectionString));

            DefaultEvents.Add(eventInfo);
        }
        else
        {
            var events = TransEvents.GetOrAdd(transObj,
                () => new DtmMessageEventList(new DbConnectionLookupInfoModel(dbContextType, CurrentTenant.Id,
                    hashedConnectionString)));

            events.Add(eventInfo);
        }
    }

    public virtual async Task PrepareAsync(CancellationToken cancellationToken = default)
    {
        var defaultMsg = GetOrCreateDtmMessage(null);
        AddEventsPublishingAction(defaultMsg, DefaultEvents);

        foreach (var (transObj, eventInfos) in TransEvents)
        {
            var msg = GetOrCreateDtmMessage(transObj);
            AddEventsPublishingAction(msg, eventInfos);
        }
        
        // WARNING:
        // the defaultMsg (with non-transactional events) will not invoke DTM's Prepare and not create a barrier.
        // That means when the DTM crashes, non-transactional events will never publish.
        // To avoid this problem, please keep using transactions if you need write-operations.

        foreach (var (transObj, dtmMessage) in TransMessages)
        {
            // todo: insert barrier for message.
            // var barrier = BranchBarrierFactory.CreateBranchBarrier(Constant.TYPE_MSG, dtmMessage.Gid,
            //     Constant.Barrier.MSG_BRANCHID, Constant.TYPE_MSG);
            //
            // if (barrier.IsInValid())
            // {
            //     throw new DtmException($"invalid trans info: {barrier}");
            // }

            await dtmMessage.Message.Prepare(GenerateQueryPreparedAddress(TransEvents[transObj]),
                cancellationToken);
        }
    }

    public virtual async Task SubmitAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (_, dtmMessage) in TransMessages)
        {
            await dtmMessage.Message.Submit(cancellationToken);
        }

        await DefaultDtmMessage.Message.Submit(cancellationToken);
    }

    protected virtual DtmMessageInfoModel GetOrCreateDtmMessage([CanBeNull] object transObj)
    {
        if (transObj is null)
        {
            if (DefaultDtmMessage == null)
            {
                var gid = GidProvider.Create();
                DefaultDtmMessage = new DtmMessageInfoModel(gid, DtmTransFactory.NewMsgGrpc(gid));
            }

            return DefaultDtmMessage;
        }
        
        TransMessages.GetOrAdd(transObj, _ =>
        {
            var gid = GidProvider.Create();
            return new DtmMessageInfoModel(gid, DtmTransFactory.NewMsgGrpc(gid));
        });

        return TransMessages[transObj];
    }
    
    protected virtual void AddEventsPublishingAction(DtmMessageInfoModel model, DtmMessageEventList eventInfos)
    {
        model.Message.Add(DtmOutboxOptions.GetPublishEventsAddress(), new DtmMsgPublishEventsRequest
        {
            ActionApiToken = DtmOutboxOptions.ActionApiToken,
            OutgoingEventInfoListToByteString = EventInfosSerializer.Serialize(eventInfos)
        });
    }
    
    protected virtual string GenerateQueryPreparedAddress(DtmMessageEventList eventList)
    {
        var baseUrl = DtmOutboxOptions.GetQueryPreparedAddress();

        var firstEvent = eventList.First();

        var extraParams =
            $"Info.DbContext={eventList.DbConnectionLookupInfo.DbContextType.FullName}&Info.TenantId={eventList.DbConnectionLookupInfo.TenantId}&Info.HashedConnectionString={eventList.DbConnectionLookupInfo.HashedConnectionString}";
        
        return baseUrl.Contains('?') ? $"{baseUrl.EnsureEndsWith('&')}{extraParams}" : $"{baseUrl}?{extraParams}";
    }
}