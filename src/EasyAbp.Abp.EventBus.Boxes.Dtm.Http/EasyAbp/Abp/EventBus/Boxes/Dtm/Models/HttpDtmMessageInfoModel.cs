using System.Collections.Generic;
using Dtmcli;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Http.EasyAbp.Abp.EventBus.Boxes.Options;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using JetBrains.Annotations;
using Volo.Abp;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Models;

public class HttpDtmMessageInfoModel : IDtmMessageInfoModel
{
    public bool EventsPublishingActionAdded { get; private set; }

    public string Gid { get; set; }

    public object DtmMessage { get; set; }

    [NotNull] public DbConnectionLookupInfoModel DbConnectionLookupInfo { get; set; }

    public List<OutgoingEventInfo> EventInfos { get; set; } = new();

    public HttpDtmMessageInfoModel(string gid, object dtmMessage,
        [NotNull] DbConnectionLookupInfoModel dbConnectionLookupInfo)
    {
        Gid = gid;
        DtmMessage = dtmMessage;
        DbConnectionLookupInfo = dbConnectionLookupInfo;
    }

    internal async Task AddEventsPublishingActionAsync(AbpDtmHttpOptions abpDtmEventBoxesOptions,
        IEventInfosSerializer serializer)
    {
        if (EventsPublishingActionAdded)
        {
            throw new AbpException("Duplicate events publishing action.");
        }

        var message = (DtmMessage as Msg)!;

        //postdata
        message.Add(abpDtmEventBoxesOptions.GetPublishEventsAddress(), new DtmMsgPublishEventsRequest
        {
            OutgoingEventInfoListToByteString = serializer.Serialize(EventInfos)
        });

        //set Headers
        var headers = new Dictionary<string, string>
        {
            {DtmRequestHeaderNames.ActionApiToken, abpDtmEventBoxesOptions.ActionApiToken},
            {
                DtmRequestHeaderNames.DbContextType,
                $"{DbConnectionLookupInfo.DbContextType.FullName}, {DbConnectionLookupInfo.DbContextType.Assembly.GetName().Name}"
            },
            {DtmRequestHeaderNames.TenantId, DbConnectionLookupInfo.TenantId.ToString()},
            {DtmRequestHeaderNames.HashedConnectionString, DbConnectionLookupInfo.HashedConnectionString},
        };
        message.SetBranchHeaders(headers);
        EventsPublishingActionAdded = true;
    }
}