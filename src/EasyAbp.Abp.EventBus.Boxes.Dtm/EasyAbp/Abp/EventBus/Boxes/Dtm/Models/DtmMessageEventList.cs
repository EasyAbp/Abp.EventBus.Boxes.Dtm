using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Models;

public class DtmMessageEventList : List<OutgoingEventInfo>
{
    [NotNull]
    public DbConnectionLookupInfoModel DbConnectionLookupInfo { get; set; }

    public DtmMessageEventList([NotNull] DbConnectionLookupInfoModel dbConnectionLookupInfo)
    {
        DbConnectionLookupInfo = dbConnectionLookupInfo;
    }
}