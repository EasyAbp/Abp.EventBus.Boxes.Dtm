using System;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers.DbSpecials;

public class SQLiteDtmBarrierDbSpecial : IDtmBarrierDbSpecial
{
    public static string DefaultBarrierTableName { get; set; } = "dtm_barrier";
    
    public static string DtmBarrierTableAndValueSqlFormat { get; set; } =
        "{0} (trans_type, gid, branch_id, op, barrier_id, reason) values (@trans_type,@gid,@branch_id,@op,@barrier_id,@reason)";
    
    public static string QueryPreparedSqlFormat { get; set; } =
        "select reason from {0} where gid=@gid and branch_id=@branch_id and op=@op and barrier_id=@barrier_id";

    public string Name { get; } = "sqlite";

    public virtual string GetCreateBarrierTableSql(AbpDtmEventBoxesOptions options)
    {
        var configuredTableName = options.BarrierTableName ?? DefaultBarrierTableName;
        var tableName = configuredTableName;

        var sql = string.Empty;

        sql += $@"
CREATE TABLE IF NOT EXISTS {tableName} (
  [id] INTEGER PRIMARY KEY AUTOINCREMENT,
  [trans_type] varchar(45) NOT NULL DEFAULT (''),
  [gid] varchar(128) NOT NULL DEFAULT (''),
  [branch_id] varchar(128) NOT NULL DEFAULT (''),
  [op] varchar(45) NOT NULL DEFAULT (''),
  [barrier_id] varchar(45) NOT NULL DEFAULT (''),
  [reason] varchar(45) NOT NULL DEFAULT (''),
  [create_time] datetime NOT NULL DEFAULT (datetime(CURRENT_TIMESTAMP)),
  [update_time] datetime NOT NULL DEFAULT (datetime(CURRENT_TIMESTAMP))
);

CREATE UNIQUE INDEX IF NOT EXISTS [ix_uniq_barrier] ON {tableName}
([gid] ASC, [branch_id] ASC, [op] ASC, [barrier_id] ASC);
";
        return sql;
    }

    public virtual string GetInsertIgnoreTemplate(string tableName)
    {
        return GetInsertIgnoreTemplate(
            string.Format(DtmBarrierTableAndValueSqlFormat, tableName ?? DefaultBarrierTableName), null);
    }

    public virtual string GetQueryPreparedSql(string tableName)
    {
        return string.Format(QueryPreparedSqlFormat, tableName ?? DefaultBarrierTableName);
    }

    public string GetPlaceHoldSQL(string sql) => sql;

    public string GetInsertIgnoreTemplate(string tableAndValues, string pgConstraint) =>
        $"insert or ignore into {tableAndValues}";

    public string GetXaSQL(string command, string xid) => throw new DtmException("not support xa now!");
}