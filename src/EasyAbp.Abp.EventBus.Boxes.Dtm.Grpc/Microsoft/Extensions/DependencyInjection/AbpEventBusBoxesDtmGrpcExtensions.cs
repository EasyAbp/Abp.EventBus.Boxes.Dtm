using System;
using Dtmgrpc;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class AbpEventBusBoxesDtmGrpcExtensions
{
    public static IServiceCollection AddAbpDtmGrpc(this IServiceCollection services,
        Action<AbpDtmGrpcOptions> setupAction)
    {
        services.AddDtmGrpc(_ => { });

        services.Configure(setupAction);

        return services;
    }
}
