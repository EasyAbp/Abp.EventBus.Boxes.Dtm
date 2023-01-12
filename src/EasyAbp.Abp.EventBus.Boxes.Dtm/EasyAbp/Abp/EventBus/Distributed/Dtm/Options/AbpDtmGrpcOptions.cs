using System;

namespace EasyAbp.Abp.EventBus.Distributed.Dtm.Options;

public class AbpDtmGrpcOptions
{
    private string _appGrpcUrl;
    private string _publishEventsPath = "/dtm_boxes.DtmGrpcService/PublishEvents";
    private string _queryPreparedPath = "/dtm_boxes.DtmGrpcService/QueryPrepared";

    /// <summary>
    /// Use this token to invoke action APIs.
    /// </summary>
    /// <returns></returns>
    public string ActionApiToken { get; set; }

    public string AppGrpcUrl
    {
        get => _appGrpcUrl;
        set => _appGrpcUrl = value.RemovePreFix("http://").RemovePreFix("https://").RemovePostFix("/");
    }

    /// <summary>
    /// dtm server grpc address, work for Dtmgrpc
    /// </summary>
    public string DtmGrpcUrl { get; set; }

    public string PublishEventsPath
    {
        get => _publishEventsPath;
        set => _publishEventsPath = value.EnsureStartsWith('/');
    }

    public string QueryPreparedPath
    {
        get => _queryPreparedPath;
        set => _queryPreparedPath = value.EnsureStartsWith('/');
    }

    public string GetPublishEventsAddress()
    {
        return $"{AppGrpcUrl}{PublishEventsPath}";
    }

    public string GetQueryPreparedAddress()
    {
        return $"{AppGrpcUrl}{QueryPreparedPath}";
    }
}