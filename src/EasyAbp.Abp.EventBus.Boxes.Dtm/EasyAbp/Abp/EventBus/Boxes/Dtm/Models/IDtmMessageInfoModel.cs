using System.Collections.Generic;
using JetBrains.Annotations;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Models;

public interface IDtmMessageInfoModel
{
    bool EventsPublishingActionAdded { get; }
    
    string Gid { get; set; }
    
    object DtmMessage { get; set; }
    
    [NotNull]
    DbConnectionLookupInfoModel DbConnectionLookupInfo { get; }

    List<OutgoingEventInfo> EventInfos { get; }
}