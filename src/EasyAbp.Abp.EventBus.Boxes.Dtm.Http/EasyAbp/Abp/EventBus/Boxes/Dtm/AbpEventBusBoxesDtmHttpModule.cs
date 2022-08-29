using EasyAbp.Abp.EventBus.Boxes.Dtm.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Modularity;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[DependsOn(
    typeof(AbpEventBusBoxesDtmModule)
)]
public class AbpEventBusBoxesDtmHttpModule : AbpModule
{
}
