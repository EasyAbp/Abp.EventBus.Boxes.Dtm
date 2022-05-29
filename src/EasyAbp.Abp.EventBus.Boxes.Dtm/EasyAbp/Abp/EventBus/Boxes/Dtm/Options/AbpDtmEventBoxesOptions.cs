using System;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Options;

public class AbpDtmEventBoxesOptions
{
    /// <summary>
    /// Use this token to invoke action APIs.
    /// </summary>
    /// <returns></returns>
    public string ActionApiToken { get; set; }
    
    public string AppGrpcUrl { get; set; }

    /// <summary>
    /// dtm server grpc address, work for Dtmgrpc
    /// </summary>
    public string DtmGrpcUrl { get; set; }

    /// <summary>
    /// The barrier table name. Recommended values:
    /// SQL Server -> DtmBarrier
    /// MySQL -> dtm_barrier
    /// PostgreSQL -> dtm.barrier
    /// </summary>
    public string BarrierTableName { get; set; } = "dtm_barrier";

    /// <summary>
    /// dtm server request timeout in milliseconds, default 10,000 milliseconds(10s)
    /// </summary>
    public int DtmTimeout { get; set; } = 10 * 1000;

    /// <summary>
    /// branch request timeout in milliseconds, default 10,000 milliseconds(10s)
    /// </summary>
    public int BranchTimeout { get; set; } = 10 * 1000;
    
    public string PublishEventsPath { get; set; } = "/dtm_boxes.DtmGrpcService/PublishEvents";
    
    public string QueryPreparedPath { get; set; } = "/dtm_boxes_efcore.DtmEfCoreGrpcService/QueryPrepared";

    public string GetPublishEventsAddress()
    {
        return $"{AppGrpcUrl.RemovePostFix("/")}{PublishEventsPath.EnsureStartsWith('/')}";
    }

    public string GetQueryPreparedAddress()
    {
        return $"{AppGrpcUrl.RemovePostFix("/")}{QueryPreparedPath.EnsureStartsWith('/')}";
    }
}
