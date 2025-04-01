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
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings["Dtm"] = MongoDbFixture.GetRandomConnectionString();
            options.ConnectionStrings["Dtm2"] = MongoDbFixture.GetRandomConnectionString();
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
                // config.Selector = null;
                config.Selector = type => type == typeof(Test2Eto);
            });

            options.Inboxes.Configure(config =>
            {
                config.UseMongoDbContextWithDtmInbox<DtmTestMongoDbContext>();
            });
        });
    }
}
