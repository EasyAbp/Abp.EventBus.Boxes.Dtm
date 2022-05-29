using System;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers.DbSpecials;

public class PostgreSQLDtmBarrierDbSpecial : PostgresDBSpecial, IDtmBarrierDbSpecial
{
    public static string DtmSequenceName { get; set; } = "barrier_seq";
    
    public virtual string GetCreateBarrierTableSql(AbpDtmEventBoxesOptions options)
    {
        var split = options.BarrierTableName.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);

        var schemaName = split.Length == 2 ? split[0] : null;
        var tableName = split.Length == 2 ? split[1] : options.BarrierTableName;
        var tableFullName = schemaName is null ? tableName : $"{schemaName}.{tableName}";
        var sequenceName = DtmSequenceName;
        var sequenceFullName = schemaName is null ? sequenceName : $"{schemaName}.{sequenceName}";

        var sql = string.Empty;

        if (schemaName is not null)
        {
            sql += $@"
create schema if not exists {schemaName};
";
        }
        
        sql += $@"
CREATE SEQUENCE if not EXISTS {sequenceFullName};
create table if not exists {tableFullName}(
  id bigint NOT NULL DEFAULT NEXTVAL ('{sequenceFullName}'),
  trans_type varchar(45) default '',
  gid varchar(128) default '',
  branch_id varchar(128) default '',
  op varchar(45) default '',
  barrier_id varchar(45) default '',
  reason varchar(45) default '',
  create_time timestamp(0) with time zone DEFAULT NULL,
  update_time timestamp(0) with time zone DEFAULT NULL,
  PRIMARY KEY(id),
  CONSTRAINT uniq_barrier unique(gid, branch_id, op, barrier_id)
);
";
        return sql;
    }
}