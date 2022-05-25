using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Studio.ModuleInstalling;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[Dependency(ServiceLifetime.Transient, ReplaceServices = true)]
[ExposeServices(typeof(IModuleInstallingPipelineBuilder))]
public class AbpEventBusBoxesDtmInstallerPipelineBuilder : ModuleInstallingPipelineBuilderBase, IModuleInstallingPipelineBuilder, ITransientDependency
{
    public async Task<ModuleInstallingPipeline> BuildAsync(ModuleInstallingContext context)
    {
        return GetBasePipeline(context);
    }
}
