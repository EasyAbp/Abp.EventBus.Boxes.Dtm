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

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = connectionString;
        });
        
        context.Services.AddMongoDbContext<DtmTestMongoDbContext>(options =>
        {
            options.AddDefaultRepositories();
        });
        
        Configure<AbpDistributedEventBusOptions>(options =>
        {
            options.Outboxes.Configure(config =>
            {
                config.UseMongoDbContextWithDtmOutbox<DtmTestMongoDbContext>();
            });

            // options.Inboxes.Configure(config =>
            // {
            //     config.UseMongoDbContextWithDtmInbox<DtmTestMongoDbContext>();
            // });
        });
    }
}
