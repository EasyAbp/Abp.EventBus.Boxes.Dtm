using EasyAbp.Abp.EventBus.Boxes.Dtm.Middleware;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderMapAbpDtmHttpServiceExtension
{
    public static void MapAbpDtmHttpService(this IApplicationBuilder app)
    {
        app.UseDtmQueryPrepared();
        app.UseDtmPublishEvents();
    }
}