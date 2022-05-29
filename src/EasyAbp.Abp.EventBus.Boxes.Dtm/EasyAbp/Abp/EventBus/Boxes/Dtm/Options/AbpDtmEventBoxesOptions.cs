﻿using System;

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
