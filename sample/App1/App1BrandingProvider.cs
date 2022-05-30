using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace App1;

[Dependency(ReplaceServices = true)]
public class App1BrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "App1";
}
