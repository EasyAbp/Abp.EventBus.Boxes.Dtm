using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.MongoDB;

[DependsOn(
    typeof(AbpEventBusBoxesDtmModule),
    typeof(AbpMongoDbModule)
)]
public class AbpEventBusBoxesDtmMongoDbModule : AbpModule
{
}
