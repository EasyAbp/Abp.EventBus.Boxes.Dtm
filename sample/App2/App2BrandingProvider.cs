using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace App2;

[Dependency(ReplaceServices = true)]
public class App2BrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "App2";
}
