using EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.Modularity;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.EntityFrameworkCore;

[DependsOn(
    typeof(AbpEventBusBoxesDtmModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class AbpEventBusBoxesDtmEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Todo: how to make the outbox not implements IDbContextEventOutbox<>? It requires the TDbContext type has OutgoingEvents property. See https://github.com/abpframework/abp/issues/12791
        context.Services.AddTransient(typeof(IDbContextEventOutbox<>), typeof(DtmDbContextEventOutbox<>));
        context.Services.AddTransient(typeof(IDtmDbContextEventOutbox<>), typeof(DtmDbContextEventOutbox<>));
    }
}
