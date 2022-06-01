using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Modularity;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.MongoDB;

[DependsOn(
    typeof(AbpEventBusBoxesDtmTestBaseModule),
    typeof(AbpEventBusBoxesDtmMongoDbModule)
    )]
public class AbpEventBusBoxesDtmMongoDbTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var stringArray = MongoDbFixture.ConnectionString.Split('?');
        var connectionString = stringArray[0].EnsureEndsWith('/') +
                               "Db_" +
                               Guid.NewGuid().ToString("N") + "/?" + stringArray[1];
        var connectionString2 = stringArray[0].EnsureEndsWith('/') +
                               "Db_" +
                               Guid.NewGuid().ToString("N") + "/?" + stringArray[1];

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings["Dtm"] = connectionString;
            options.ConnectionStrings["Dtm2"] = connectionString2;
        });
        
        context.Services.AddMongoDbContext<DtmTestMongoDbContext>(options =>
        {
            options.AddDefaultRepositories();
        });
        
        context.Services.AddMongoDbContext<DtmTestMongoDbContext2>(options =>
        {
            options.AddDefaultRepositories();
        });
        
        Configure<AbpDistributedEventBusOptions>(options =>
        {
            options.Outboxes.Configure("FirstBox", config =>
            {
                config.UseMongoDbContextWithDtmOutbox<DtmTestMongoDbContext>();
                config.Selector = type => type == typeof(TestEto);
            });
            
            options.Outboxes.Configure("SecondBox", config =>
            {
                config.UseMongoDbContextWithDtmOutbox<DtmTestMongoDbContext2>();
                config.Selector = null;
                // config.Selector = type => type == typeof(Test2Eto);
            });

            // options.Inboxes.Configure(config =>
            // {
            //     config.UseMongoDbContextWithDtmInbox<DtmTestMongoDbContext>();
            // });
        });
    }
}
