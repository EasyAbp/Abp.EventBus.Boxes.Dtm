using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(AbpAuthorizationModule),
    typeof(AbpEventBusBoxesDtmModule)
    )]
public class AbpEventBusBoxesDtmTestBaseModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAlwaysAllowAuthorization();
        
        context.Services.AddDtmBoxes();
        
        context.Services.TryAddTransient<TestDtmUnitOfWork>();
        context.Services.Replace(ServiceDescriptor.Transient<IUnitOfWork, TestDtmUnitOfWork>());
        context.Services.Replace(ServiceDescriptor.Transient<DtmUnitOfWork, TestDtmUnitOfWork>());
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        SeedTestData(context);
    }

    private static void SeedTestData(ApplicationInitializationContext context)
    {
        AsyncHelper.RunSync(async () =>
        {
            using (var scope = context.ServiceProvider.CreateScope())
            {
                await scope.ServiceProvider
                    .GetRequiredService<IDataSeeder>()
                    .SeedAsync();
            }
        });
    }
}
