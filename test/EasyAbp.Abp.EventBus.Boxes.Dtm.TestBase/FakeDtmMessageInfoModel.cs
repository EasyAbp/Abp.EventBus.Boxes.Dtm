using System.Collections.Generic;
using EasyAbp.Abp.EventBus.Distributed.Dtm.Models;
using JetBrains.Annotations;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class FakeDtmMessageInfoModel : IDtmMessageInfoModel
{
    public bool EventsPublishingActionAdded { get; }
    
    public string Gid { get; set; }
    
    public object DtmMessage { get; set; }
    
    public DbConnectionLookupInfoModel DbConnectionLookupInfo { get; }

    public List<OutgoingEventInfo> EventInfos { get; } = new();

    public FakeDtmMessageInfoModel(string gid, object dtmMessage,
        [NotNull] DbConnectionLookupInfoModel dbConnectionLookupInfo)
    {
        Gid = gid;
        DtmMessage = dtmMessage;
        DbConnectionLookupInfo = dbConnectionLookupInfo;
    }
}