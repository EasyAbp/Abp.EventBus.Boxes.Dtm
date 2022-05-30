using System.Collections.Generic;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers.DbSpecials;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;

public static class BarrierSqlTemplates
{
    private static IDtmBarrierDbSpecial MySQLDbSpecial { get; set; } = new MySQLDtmBarrierDbSpecial();
    private static IDtmBarrierDbSpecial PostgreSQLDbSpecial { get; set; } = new PostgreSQLDtmBarrierDbSpecial();
    private static IDtmBarrierDbSpecial SQLServerDbSpecial { get; set; } = new SQLServerDtmBarrierDbSpecial();

    public static Dictionary<string, IDtmBarrierDbSpecial> DbProviderSpecialMapping { get; } = new()
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