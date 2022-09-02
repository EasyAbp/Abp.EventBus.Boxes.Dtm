using EasyAbp.Abp.EventBus.Boxes.Dtm.Middleware;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderMapAbpDtmHttpServiceExtension
{
    public static void UseAbpDtmEventBoxesHttpServices(this IApplicationBuilder app)
    {
        app.UseDtmQueryPrepared();
        app.UseDtmPublishEvents();
    }
}