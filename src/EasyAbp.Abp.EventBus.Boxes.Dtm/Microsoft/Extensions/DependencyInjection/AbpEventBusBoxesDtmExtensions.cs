using System;
using DtmCommon;
using Dtmgrpc;
using Dtmgrpc.Driver;
using EasyAbp.Abp.EventBus.Boxes.Dtm;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.EventBus.Boxes;
using Volo.Abp.Uow;

namespace Microsoft.Extensions.DependencyInjection;

public static class AbpEventBusBoxesDtmExtensions
{
    public static IServiceCollection AddDtmOutbox(this IServiceCollection services, Action<DtmOutboxOptions> setupAction)
    {
        services.Configure(setupAction);

        services.AddTransient<DtmUnitOfWork>();
        services.AddTransient<NullOutboxSender>();
        services.Replace(ServiceDescriptor.Transient<IUnitOfWork, DtmUnitOfWork>());
        services.Replace(ServiceDescriptor.Transient<IOutboxSender, NullOutboxSender>());
        
        services.AddAbpDtmGrpc(setupAction);
        
        return services;
    }

    public static IServiceCollection AddAbpDtmGrpc(this IServiceCollection services, Action<DtmOutboxOptions> setupAction)
    {
        services.TryAddSingleton<IDtmDriver, DefaultDtmDriver>();
        services.TryAddSingleton<IDtmTransFactory, DtmTransFactory>();
        services.TryAddSingleton<IDtmgRPCClient, DtmgRPCClient>();
        
        // Todo: map DtmOutboxOptions to and configure the DtmOptions.

        return services;
    }
}
