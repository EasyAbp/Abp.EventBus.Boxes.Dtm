using Volo.Abp.AspNetCore;
using Volo.Abp.Modularity;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[DependsOn(
    typeof(AbpEventBusBoxesDtmModule),
    typeof(AbpAspNetCoreModule)
)]
public class AbpEventBusBoxesDtmGrpcModule : AbpModule
{
}
