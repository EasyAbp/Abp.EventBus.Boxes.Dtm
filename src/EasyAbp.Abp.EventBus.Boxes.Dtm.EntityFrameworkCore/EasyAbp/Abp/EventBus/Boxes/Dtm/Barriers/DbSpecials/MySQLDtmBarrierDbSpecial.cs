using System;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers.DbSpecials;

public class MySQLDtmBarrierDbSpecial : MysqlDBSpecial, IDtmBarrierDbSpecial
{
    public static string DefaultBarrierTableName { get; set; } = "dtm_barrier";

    public static string DtmBarrierTableAndValueSqlFormat { get; set; } =
        "{0}(trans_type, gid, branch_id, op, barrier_id, reason) values(@trans_type,@gid,@branch_id,@op,@barrier_id,@reason)";
    
    public static string QueryPreparedSqlFormat { get; set; } =
        "select reason from {0} where gid=@gid and branch_id=@branch_id and op=@op and barrier_id=@barrier_id";
    
    public virtual string GetCreateBarrierTableSql(AbpDtmEventBoxesOptions options)
    {
        var configuredTableName = options.BarrierTableName ?? DefaultBarrierTableName;
        var split = configuredTableName.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);

        var databaseName = split.Length == 2 ? split[0] : null;
        var tableName = split.Length == 2 ? split[1] : configuredTableName;
        var tableFullName = databaseName is null ? tableName : $"{databaseName}.{tableName}";

        var sql = string.Empty;

        if (databaseName is not null)
        {
            sql += $@"
create database if not exists {databaseName}
/*!40100 DEFAULT CHARACTER SET utf8mb4 */
;
";
        }
        
        sql += $@"
create table if not exists {tableFullName}(
  id bigint(22) PRIMARY KEY AUTO_INCREMENT,
  trans_type varchar(45) default '',
  gid varchar(128) default '',
  branch_id varchar(128) default '',
  op varchar(45) default '',
  barrier_id varchar(45) default '',
  reason varchar(45) default '' comment 'the branch type who insert this record',
  create_time datetime DEFAULT now(),
  update_time datetime DEFAULT now(),
  key(create_time),
  key(update_time),
  UNIQUE key(gid, branch_id, op, barrier_id)
);
";
        return sql;
    }
    
    public virtual string GetInsertIgnoreTemplate(string tableName)
    {
        return base.GetInsertIgnoreTemplate(
            string.Format(DtmBarrierTableAndValueSqlFormat, tableName ?? DefaultBarrierTableName), null);
    }

    public virtual string GetQueryPreparedSql(string tableName)
    {
        return string.Format(QueryPreparedSqlFormat, tableName ?? DefaultBarrierTableName);
    }
}