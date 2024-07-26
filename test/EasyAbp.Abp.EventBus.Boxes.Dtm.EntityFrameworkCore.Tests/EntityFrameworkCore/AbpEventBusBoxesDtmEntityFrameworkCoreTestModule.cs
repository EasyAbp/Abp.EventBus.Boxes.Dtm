using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Modularity;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.EntityFrameworkCore;

[DependsOn(
    typeof(AbpEventBusBoxesDtmTestBaseModule),
    typeof(AbpEventBusBoxesDtmEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
    )]
public class AbpEventBusBoxesDtmEntityFrameworkCoreTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var sqliteConnection = CreateDatabaseAndGetConnection();

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<DtmTestDbContext>(abpDbContextConfigurationContext =>
            {
                abpDbContextConfigurationContext.DbContextOptions.UseSqlite(sqliteConnection);
            });
        });

        var sqliteConnection2 = CreateDatabaseAndGetConnection2();

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<DtmTestDbContext2>(abpDbContextConfigurationContext =>
            {
                abpDbContextConfigurationContext.DbContextOptions.UseSqlite(sqliteConnection2);
            });
        });
        
        context.Services.AddAbpDbContext<DtmTestDbContext>(options =>
        {
            options.AddDefaultRepositories();
        });
        
        context.Services.AddAbpDbContext<DtmTestDbContext2>(options =>
        {
            options.AddDefaultRepositories();
        });
        
        Configure<AbpDistributedEventBusOptions>(options =>
        {
            options.Outboxes.Configure("FirstBox", config =>
            {
                config.UseDbContextWithDtmOutbox<DtmTestDbContext>();
                config.Selector = type => type == typeof(TestEto);
            });
            
            options.Outboxes.Configure("SecondBox", config =>
            {
                config.UseDbContextWithDtmOutbox<DtmTestDbContext2>();
                config.Selector = null;
                // config.Selector = type => type == typeof(Test2Eto);
            });

            options.Inboxes.Configure(config =>
            {
                config.UseDbContextWithDtmInbox<DtmTestDbContext>();
            });
        });
        
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings["Dtm"] = "Data Source=Context;Mode=Memory;Cache=Shared";
            options.ConnectionStrings["Dtm2"] = "Data Source=Context2;Mode=Memory;Cache=Shared";
        });
    }

    private static SqliteConnection CreateDatabaseAndGetConnection()
    {
        var connection = new AbpUnitTestSqliteConnection("Data Source=Context;Mode=Memory;Cache=Shared");
        connection.Open();

        new DtmTestDbContext(
            new DbContextOptionsBuilder<DtmTestDbContext>().UseSqlite(connection).Options
        ).GetService<IRelationalDatabaseCreator>().CreateTables();

        return connection;
    }

    private static SqliteConnection CreateDatabaseAndGetConnection2()
    {
        var connection = new AbpUnitTestSqliteConnection("Data Source=Context2;Mode=Memory;Cache=Shared");
        connection.Open();

        new DtmTestDbContext2(
            new DbContextOptionsBuilder<DtmTestDbContext2>().UseSqlite(connection).Options
        ).GetService<IRelationalDatabaseCreator>().CreateTables();

        return connection;
    }
}
