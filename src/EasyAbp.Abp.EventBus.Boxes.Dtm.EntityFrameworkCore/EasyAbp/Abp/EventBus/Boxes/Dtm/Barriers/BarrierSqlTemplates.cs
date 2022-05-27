using System.Collections.Generic;
using DtmCommon;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;

public static class BarrierSqlTemplates
{
    private static readonly IDbSpecial MySQLDbSpecial = new MysqlDBSpecial();
    private static readonly IDbSpecial PostgreSQLDbSpecial = new PostgresDBSpecial();
    private static readonly IDbSpecial SQLServerDbSpecial = new SqlServerDBSpecial();

    public static string DtmBarrierTableAndValueSqlFormat { get; set; } =
        "{0}(trans_type, gid, branch_id, op, barrier_id, reason) values(@trans_type,@gid,@branch_id,@op,@barrier_id,@reason)";
    
    public static string QueryPreparedSqlFormat { get; set; } =
        "select reason from {0} where gid=@gid and branch_id=@branch_id and op=@op and barrier_id=@barrier_id";

    public static Dictionary<string, IDbSpecial> DbProviderSpecialMapping { get; } = new()
    {
        // MySQL
        { "Pomelo.EntityFrameworkCore.MySql", MySQLDbSpecial },
        { "MySql.EntityFrameworkCore", MySQLDbSpecial },
        { "Devart.Data.MySql.EFCore", MySQLDbSpecial },
        
        // PostgreSQL
        { "Npgsql.EntityFrameworkCore.PostgreSQL", PostgreSQLDbSpecial },
        { "Devart.Data.PostgreSql.EFCore", PostgreSQLDbSpecial },
        
        // SQL Server
        { "Microsoft.EntityFrameworkCore.SqlServer", SQLServerDbSpecial },
    };
}