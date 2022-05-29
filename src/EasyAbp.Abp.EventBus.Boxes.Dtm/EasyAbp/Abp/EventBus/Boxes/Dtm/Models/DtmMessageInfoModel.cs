﻿using System;
using System.Collections.Generic;
using Dtmgrpc;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using JetBrains.Annotations;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Models;

public class DtmMessageInfoModel
{
    public bool EventsPublishingActionAdded { get; private set; }
    
    public string Gid { get; set; }
    
    public MsgGrpc DtmMessage { get; set; }
    
    [NotNull]
    public DbConnectionLookupInfoModel DbConnectionLookupInfo { get; set; }

    public List<OutgoingEventInfo> EventInfos { get; set; } = new();

    public DtmMessageInfoModel(string gid, MsgGrpc dtmMessage,
        [NotNull] DbConnectionLookupInfoModel dbConnectionLookupInfo)
    {
        Gid = gid;
        DtmMessage = dtmMessage;
        DbConnectionLookupInfo = dbConnectionLookupInfo;
    }

    internal void AddEventsPublishingAction(AbpDtmEventBoxesOptions abpDtmEventBoxesOptions, IEventInfosSerializer serializer)
    {
        if (EventsPublishingActionAdded)
        {
            throw new ApplicationException("Duplicate events publishing action.");
        }
        
        DtmMessage.Add(abpDtmEventBoxesOptions.GetPublishEventsAddress(), new DtmMsgPublishEventsRequest
        {
            ActionApiToken = abpDtmEventBoxesOptions.ActionApiToken,
            OutgoingEventInfoListToByteString = serializer.Serialize(EventInfos)
        });

        EventsPublishingActionAdded = true;
    }
}