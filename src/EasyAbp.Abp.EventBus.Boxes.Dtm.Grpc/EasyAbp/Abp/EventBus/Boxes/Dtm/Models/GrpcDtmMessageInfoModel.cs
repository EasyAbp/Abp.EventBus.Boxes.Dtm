using System.Collections.Generic;
using Dtmgrpc;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using JetBrains.Annotations;
using Volo.Abp;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Models;

public class GrpcDtmMessageInfoModel : IDtmMessageInfoModel
{
    public bool EventsPublishingActionAdded { get; private set; }
    
    public string Gid { get; set; }
    
    public object DtmMessage { get; set; }
    
    [NotNull]
    public DbConnectionLookupInfoModel DbConnectionLookupInfo { get; set; }

    public List<OutgoingEventInfo> EventInfos { get; set; } = new();

    public GrpcDtmMessageInfoModel(string gid, object dtmMessage,
        [NotNull] DbConnectionLookupInfoModel dbConnectionLookupInfo)
    {
        Gid = gid;
        DtmMessage = dtmMessage;
        DbConnectionLookupInfo = dbConnectionLookupInfo;
    }

    internal void AddEventsPublishingAction(AbpDtmGrpcOptions abpDtmEventBoxesOptions, IEventInfosSerializer serializer)
    {
        if (EventsPublishingActionAdded)
        {
            throw new AbpException("Duplicate events publishing action.");
        }
        
        var message = (DtmMessage as MsgGrpc)!;
        
        message.Add(abpDtmEventBoxesOptions.GetPublishEventsAddress(), new DtmMsgPublishEventsRequest
        {
            OutgoingEventInfoListToByteString = serializer.Serialize(EventInfos)
        });

        var dbContextType =
            $"{DbConnectionLookupInfo.DbContextType.FullName}, {DbConnectionLookupInfo.DbContextType.Assembly.GetName().Name}";

        message.SetBranchHeaders(new Dictionary<string, string>
        {
            {DtmRequestHeaderNames.ActionApiToken, abpDtmEventBoxesOptions.ActionApiToken},
            {DtmRequestHeaderNames.DbContextType, dbContextType},
            {DtmRequestHeaderNames.TenantId, DbConnectionLookupInfo.TenantId.ToString()},
            {DtmRequestHeaderNames.HashedConnectionString, DbConnectionLookupInfo.HashedConnectionString},
        });

        EventsPublishingActionAdded = true;
    }
}