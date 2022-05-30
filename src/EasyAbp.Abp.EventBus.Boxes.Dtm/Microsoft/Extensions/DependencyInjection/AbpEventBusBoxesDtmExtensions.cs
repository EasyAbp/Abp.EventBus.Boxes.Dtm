using System;
using EasyAbp.Abp.EventBus.Boxes.Dtm;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.EventBus.Boxes;
using Volo.Abp.Uow;

namespace Microsoft.Extensions.DependencyInjection;

public static class AbpEventBusBoxesDtmExtensions
{
    public static IServiceCollection AddDtmBoxes(this IServiceCollection services,
        Action<AbpDtmEventBoxesOptions> setupAction)
    {
        ConfigureDtmEventBoxes(services, setupAction);

        return AddDtmBoxes(services);
    }
    
    public static IServiceCollection AddDtmBoxes(this IServiceCollection services)
    {
        AddDtmOutbox(services);
        AddDtmInbox(services);

        return services;
    }
    
    public static IServiceCollection AddDtmOutbox(this IServiceCollection services,
        Action<AbpDtmEventBoxesOptions> setupAction)
    {
        ConfigureDtmEventBoxes(services, setupAction);

        return AddDtmOutbox(services);
    }
    
    private static IServiceCollection AddDtmOutbox(this IServiceCollection services)
    {
        services.TryAddTransient<DtmUnitOfWork>();
        services.TryAddTransient<NullOutboxSender>();
        services.Replace(ServiceDescriptor.Transient<IUnitOfWork, DtmUnitOfWork>());
        services.Replace(ServiceDescriptor.Transient<IOutboxSender, NullOutboxSender>());

        return services;
    }
    
    public static IServiceCollection AddDtmInbox(this IServiceCollection services,
        Action<AbpDtmEventBoxesOptions> setupAction)
    {
        ConfigureDtmEventBoxes(services, setupAction);

        return AddDtmInbox(services);
    }
    
    private static IServiceCollection AddDtmInbox(this IServiceCollection services)
    {
        return services;
    }

    public static void ConfigureDtmEventBoxes(this IServiceCollection services,
        Action<AbpDtmEventBoxesOptions> setupAction)
    {
        services.Configure(setupAction);
    }
}
