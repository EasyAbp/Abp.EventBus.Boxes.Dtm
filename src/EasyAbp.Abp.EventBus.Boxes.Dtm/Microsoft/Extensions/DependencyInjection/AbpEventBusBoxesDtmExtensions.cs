using System;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class AbpEventBusBoxesDtmExtensions
{
    public static IServiceCollection AddDtmOutbox(this IServiceCollection services, Action<DtmOutboxOptions> setupAction)
    {
        return services;
    }
}
