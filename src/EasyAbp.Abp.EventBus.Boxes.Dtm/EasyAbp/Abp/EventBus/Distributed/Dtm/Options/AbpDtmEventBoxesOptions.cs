namespace EasyAbp.Abp.EventBus.Distributed.Dtm.Options;

public class AbpDtmEventBoxesOptions
{
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
}
