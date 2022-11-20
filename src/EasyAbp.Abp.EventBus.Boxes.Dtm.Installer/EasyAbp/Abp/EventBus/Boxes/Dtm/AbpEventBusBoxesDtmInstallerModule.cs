using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[DependsOn(
    typeof(AbpVirtualFileSystemModule)
    )]
public class AbpEventBusBoxesDtmInstallerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpEventBusBoxesDtmInstallerModule>();
        });
    }
}
