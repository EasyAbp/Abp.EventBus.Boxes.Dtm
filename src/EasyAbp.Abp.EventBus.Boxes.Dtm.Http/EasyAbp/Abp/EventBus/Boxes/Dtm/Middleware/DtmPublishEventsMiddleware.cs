using System.Net;
using System.Text.Json;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Http.EasyAbp.Abp.EventBus.Boxes.Options;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Models;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.EventBus.Boxes;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Json;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Middleware;

/// <summary>
/// dtm message receive
/// </summary>
public class DtmPublishEventsMiddleware:BaseDtmServiceMiddleware
{
    private readonly RequestDelegate _next;
    public DtmPublishEventsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="distributedEventBus"></param>
    /// <param name="eventInfosSerializer"></param>
    /// <param name="abpDtmOptions"></param>
    /// <param name="logger"></param>
    public async Task InvokeAsync(HttpContext context,
        IDistributedEventBus distributedEventBus,
        IEventInfosSerializer eventInfosSerializer,IOptions<AbpDtmHttpOptions> abpDtmOptions,ILogger<DtmPublishEventsMiddleware> logger)
    {
        //check url
        if (!context.Request.Path.StartsWithSegments(abpDtmOptions.Value.PublishEventsPath,StringComparison.OrdinalIgnoreCase))
        {
           await _next(context);
           return;
        }
        logger.LogTrace("receive dtm publish message");
        context.Request.EnableBuffering();
        await CheckActionApiTokenAsync(context);

        var supportsEventBoxes = distributedEventBus.AsSupportsEventBoxes();
        
        using var streamReader = new StreamReader(context.Request.Body);
        var body = await streamReader.ReadToEndAsync();
        var jsonSerializer= context.RequestServices.GetRequiredService<IEventInfosSerializer>();
        var dtmMsgPublishEventsRequest = JsonSerializer.Deserialize<DtmMsgPublishEventsRequest>(body);
        var eventInfos = jsonSerializer.Deserialize(dtmMsgPublishEventsRequest.OutgoingEventInfoListToByteString);
        try
        {
            await supportsEventBoxes.PublishManyFromOutboxAsync(eventInfos, new OutboxConfig("DTM_Empty"));
        }
        catch (Exception e)
        {
            logger.LogError("receive dtm publish message error");
            throw;
        }
        context.Response.StatusCode = (int) HttpStatusCode.OK;
    }
}

public static class DtmPublishEventsMiddlewareExtension
{
    public static void UseDtmPublishEvents(this IApplicationBuilder app)
    {
        app.UseMiddleware<DtmPublishEventsMiddleware>();
    }
} 