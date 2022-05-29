using System;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers.DbSpecials;

public class MySQLDtmBarrierDbSpecial : MysqlDBSpecial, IDtmBarrierDbSpecial
{
    public virtual string GetCreateBarrierTableSql(AbpDtmEventBoxesOptions options)
    {
        var split = options.BarrierTableName.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);

        var databaseName = split.Length == 2 ? split[0] : null;
        var tableName = split.Length == 2 ? split[1] : options.BarrierTableName;
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
}