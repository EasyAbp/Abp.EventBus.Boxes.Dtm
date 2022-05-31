using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderMapAbpDtmGrpcServiceExtension
{
    public static GrpcServiceEndpointConventionBuilder MapAbpDtmGrpcService(this IEndpointRouteBuilder builder)
    {
        return builder.MapGrpcService<EasyAbp.Abp.EventBus.Boxes.Dtm.Services.DtmGrpcService>();

    }
}