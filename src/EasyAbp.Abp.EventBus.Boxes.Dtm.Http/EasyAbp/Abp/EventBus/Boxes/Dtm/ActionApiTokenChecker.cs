using EasyAbp.Abp.EventBus.Boxes.Dtm.Http.EasyAbp.Abp.EventBus.Boxes.Options;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IActionApiTokenChecker))]
public class ActionApiTokenChecker: IActionApiTokenChecker, ISingletonDependency
{
    private readonly IOptions<AbpDtmHttpOptions> _abpDtmHttpOptions;
    public ActionApiTokenChecker(IOptions<AbpDtmHttpOptions> abpDtmHttpOptions)
    {
        _abpDtmHttpOptions = abpDtmHttpOptions;
    }
    public Task<bool> IsCorrectAsync(string token)
    {
        return Task.FromResult(_abpDtmHttpOptions.Value.ActionApiToken == token);
    }
}