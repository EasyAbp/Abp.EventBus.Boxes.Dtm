using EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.DistributedEvents;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.MongoDB;

[DependsOn(
    typeof(AbpEventBusBoxesDtmModule),
    typeof(AbpMongoDbModule)
)]
public class AbpEventBusBoxesDtmMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Todo: how to make the outbox not implements IMongoDbContextEventOutbox<>? It requires the TDbContext type has OutgoingEvents property. See https://github.com/abpframework/abp/issues/12791
        context.Services.AddTransient(typeof(IMongoDbContextEventOutbox<>), typeof(DtmMongoDbContextEventOutbox<>));
        context.Services.AddTransient(typeof(IDtmMongoDbContextEventOutbox<>), typeof(DtmMongoDbContextEventOutbox<>));
    }
}
