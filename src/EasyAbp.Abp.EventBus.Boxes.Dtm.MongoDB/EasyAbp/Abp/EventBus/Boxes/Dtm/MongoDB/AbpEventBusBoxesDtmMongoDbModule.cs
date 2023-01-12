using EasyAbp.Abp.EventBus.Boxes.Dtm.Inbox;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;
using EasyAbp.Abp.EventBus.Distributed.Dtm;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.MongoDB;

[DependsOn(
    typeof(AbpEventBusBoxesDtmModule),
    typeof(AbpMongoDbModule)
)]
public class AbpEventBusBoxesDtmMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient(typeof(IDtmMongoDbContextEventOutbox<>), typeof(DtmMongoDbContextEventOutbox<>));
        context.Services.AddTransient(typeof(IDtmMongoDbContextEventInbox<>), typeof(DtmMongoDbContextEventInbox<>));
    }
}
