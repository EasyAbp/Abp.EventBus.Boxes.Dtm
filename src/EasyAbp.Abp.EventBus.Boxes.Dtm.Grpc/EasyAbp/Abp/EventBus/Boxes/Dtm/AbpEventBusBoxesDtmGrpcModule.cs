using Volo.Abp.Modularity;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[DependsOn(
    typeof(AbpEventBusBoxesDtmModule)
)]
public class AbpEventBusBoxesDtmGrpcModule : AbpModule
{
}
