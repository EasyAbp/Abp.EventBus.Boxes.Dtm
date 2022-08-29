using System.Net;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Http.EasyAbp.Abp.EventBus.Boxes.Options;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Middleware;

/// <summary>
/// 
/// </summary>
public class DtmQueryPreparedMiddleware : BaseDtmServiceMiddleware
{
    private readonly RequestDelegate _next;
    public DtmQueryPreparedMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="AbpException"></exception>
    public async Task InvokeAsync(HttpContext context, ICurrentTenant currentTenant,
        IServiceProvider serviceProvider,
        IDistributedEventBus distributedEventBus,
        IEventInfosSerializer eventInfosSerializer,
        IUnitOfWorkManager unitOfWorkManager,IOptions<AbpDtmHttpOptions> abpDtmOptions)
    {
        //check url
        if (!context.Request.Path.StartsWithSegments(abpDtmOptions.Value.QueryPreparedPath,StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }
        await CheckActionApiTokenAsync(context);

        if (!context.Request.Headers.TryGetValue("dtm-gid", out var gid))
        {
            throw new AbpException("Cannot get dtm-gid from the http request headers.");
        }
        context.Request.Headers.TryGetValue(DtmRequestHeaderNames.TenantId, out var tenantIdString);
        var tenantId = tenantIdString.IsNullOrEmpty() ? (Guid?) null : Guid.Parse(tenantIdString);

        using var unitOfWork = unitOfWorkManager.Begin(true);
        using var changeTenant = currentTenant.Change(tenantId);

        var dbContextTypeName = context.Request.Headers[DtmRequestHeaderNames.DbContextType];
        var hashedConnectionString = context.Request.Headers[DtmRequestHeaderNames.HashedConnectionString];

        var handlers = context.RequestServices.GetServices<IDtmQueryPreparedHandler>();

        foreach (var handler in handlers)
        {
            if (!await handler.CanHandleAsync(dbContextTypeName))
            {
                continue;
            }

            if (await handler.TryInsertBarrierAsRollbackAsync(dbContextTypeName, hashedConnectionString, gid))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                return;
            }

            await unitOfWork.CompleteAsync();
        }

        throw new AbpException(
            $"Cannot find a DTM query prepared handler for the DbContext type {dbContextTypeName}");
    }
}
public static class DtmQueryPreparedMiddlewareExtension
{
    public static void UseDtmQueryPrepared(this IApplicationBuilder app)
    {
        app.UseMiddleware<DtmQueryPreparedMiddleware>();
    }
}