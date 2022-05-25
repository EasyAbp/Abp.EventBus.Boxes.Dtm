using System;
using JetBrains.Annotations;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Options;

public class DtmOutboxOptions
{
    /// <summary>
    /// Use this token to invoke action APIs.
    /// </summary>
    /// <returns></returns>
    [NotNull]
    public string ActionApiToken { get; set; }
    
    [NotNull]
    public string GrpcHost { get; set; }

    [NotNull]
    public string PublishEventsPath { get; set; } = "/DtmApi/PublishEvents";
    
    [NotNull]
    public string QueryPreparedPath { get; set; } = "/DtmApi/QueryPrepared";

    public DtmOutboxOptions([NotNull] string actionApiToken, [NotNull] string grpcHost)
    {
        ActionApiToken = actionApiToken;
        GrpcHost = grpcHost;
    }

    public string GetPublishEventsAddress()
    {
        return $"{GrpcHost.RemovePostFix("/")}{PublishEventsPath.EnsureStartsWith('/')}";
    }

    public string GetQueryPreparedAddress()
    {
        return $"{GrpcHost.RemovePostFix("/")}{QueryPreparedPath.EnsureStartsWith('/')}";
    }
}