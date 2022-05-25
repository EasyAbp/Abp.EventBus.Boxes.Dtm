using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.EntityFrameworkCore;

[DependsOn(
    typeof(AbpEventBusBoxesDtmModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class AbpEventBusBoxesDtmEntityFrameworkCoreModule : AbpModule
{
}
