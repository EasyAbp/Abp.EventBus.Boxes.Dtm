using EasyAbp.Abp.EventBus.Boxes.Dtm.Inbox;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;
using EasyAbp.Abp.EventBus.Distributed.Dtm;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
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
        context.Services.AddTransient(typeof(IDtmDbContextEventOutbox<>), typeof(DtmDbContextEventOutbox<>));
        context.Services.AddTransient(typeof(IDtmDbContextEventInbox<>), typeof(DtmDbContextEventInbox<>));
    }
}
