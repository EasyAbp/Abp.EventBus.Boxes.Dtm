using System;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Options;

public class AbpDtmEventBoxesOptions
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

    /// <summary>
    /// The barrier table name. It will use the default value if you keep it <c>null</c>:<br /><br />
    /// SQL Server -> dtm.Barrier<br />
    /// MySQL -> dtm_barrier<br />
    /// PostgreSQL -> dtm.barrier<br />
    /// MongoDB -> dtm_barrier
    /// </summary>
    public string BarrierTableName { get; set; } = null;

    /// <summary>
    /// dtm server request timeout in milliseconds, default 10,000 milliseconds(10s)
    /// </summary>
    public int DtmTimeout { get; set; } = 10 * 1000;

    /// <summary>
    /// branch request timeout in milliseconds, default 10,000 milliseconds(10s)
    /// </summary>
    public int BranchTimeout { get; set; } = 10 * 1000;
    
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
