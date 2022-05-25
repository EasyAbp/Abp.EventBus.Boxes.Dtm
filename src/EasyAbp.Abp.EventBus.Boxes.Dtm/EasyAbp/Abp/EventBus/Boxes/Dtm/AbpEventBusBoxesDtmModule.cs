using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[DependsOn(
    typeof(AbpEventBusModule)
)]
public class AbpEventBusBoxesDtmModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
    }
}
