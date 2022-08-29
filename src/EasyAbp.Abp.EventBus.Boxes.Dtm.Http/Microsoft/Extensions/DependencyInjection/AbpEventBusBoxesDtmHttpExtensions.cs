using System;
using Dtmcli;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Http.EasyAbp.Abp.EventBus.Boxes.Options;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class AbpEventBusBoxesDtmHttpExtensions
{
    public static IServiceCollection AddAbpDtmHttp(this IServiceCollection services,
        Action<AbpDtmHttpOptions> setupAction)
    {
        services.AddDtmcli((_) => { });
        services.Configure(setupAction);
        var dtmHttpOptions = new AbpDtmHttpOptions();
        setupAction.Invoke(dtmHttpOptions);
        services.AddHttpClient("dtmClient", options =>
        {
            options.BaseAddress = new Uri(dtmHttpOptions.DtmUrl);
            options.Timeout=TimeSpan.FromSeconds(dtmHttpOptions.Timeout);
        });
        return services;
    }
}
