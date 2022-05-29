using System;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers.DbSpecials;

public class SQLServerDtmBarrierDbSpecial : SqlServerDBSpecial, IDtmBarrierDbSpecial
{
    public static string DefaultBarrierTableName { get; set; } = "dtm.Barrier";
    
    public virtual string GetCreateBarrierTableSql(AbpDtmEventBoxesOptions options)
    {
        var configuredTableName = options.BarrierTableName ?? DefaultBarrierTableName;
        var split = configuredTableName.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);

        var schemaName = split.Length == 2 ? split[0] : null;
        var tableName = split.Length == 2 ? split[1] : configuredTableName;
        var tableFullName = schemaName is null ? tableName : $"{schemaName}.{tableName}";

        var sql = string.Empty;

        if (schemaName is not null)
        {
            sql += $@"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{schemaName}')
    BEGIN
	    EXEC('CREATE SCHEMA [{schemaName}]')
    END

";
        }
        
        sql += $@"
IF OBJECT_ID(N'{tableFullName}',N'U') IS NULL
    BEGIN
        CREATE TABLE {tableFullName}
        (
           [id] bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
           [trans_type] varchar(45) NOT NULL DEFAULT(''),
           [gid] varchar(128) NOT NULL DEFAULT(''),
           [branch_id] varchar(128) NOT NULL DEFAULT(''),
           [op] varchar(45) NOT NULL DEFAULT(''),
           [barrier_id] varchar(45) NOT NULL DEFAULT(''),
           [reason] varchar(45) NOT NULL DEFAULT(''),
           [create_time] datetime NOT NULL DEFAULT(getdate()) ,
           [update_time] datetime NOT NULL DEFAULT(getdate())
        )

        CREATE UNIQUE INDEX[ix_uniq_barrier] ON {tableFullName}
               ([gid] ASC, [branch_id] ASC, [op] ASC, [barrier_id] ASC)
        WITH(IGNORE_DUP_KEY = ON)
    END
";
        Console.WriteLine(sql);
        return sql;
    }
}