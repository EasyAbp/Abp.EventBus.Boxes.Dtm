using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Middleware;

public class BaseDtmServiceMiddleware
{
    /// <summary>
    /// Verify apitoken
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="AbpException"></exception>
    protected virtual async Task CheckActionApiTokenAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(DtmRequestHeaderNames.ActionApiToken, out var actionApiToken))
        {
            var actionApiTokenChecker = context.RequestServices.GetRequiredService<IActionApiTokenChecker>();
            if (!await actionApiTokenChecker.IsCorrectAsync(actionApiToken))
            {
                throw new AbpException("Incorrect ActionApiToken!");
            }
        }
    }
}