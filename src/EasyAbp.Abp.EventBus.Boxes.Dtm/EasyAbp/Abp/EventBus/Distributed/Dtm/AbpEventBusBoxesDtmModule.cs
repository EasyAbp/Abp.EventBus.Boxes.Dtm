using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace EasyAbp.Abp.EventBus.Distributed.Dtm;

[DependsOn(
    typeof(AbpEventBusModule)
)]
public class AbpEventBusBoxesDtmModule : AbpModule
{
}
