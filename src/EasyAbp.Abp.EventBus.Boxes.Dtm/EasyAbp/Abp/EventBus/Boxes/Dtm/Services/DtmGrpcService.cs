using System;
using System.Linq;
using System.Threading.Tasks;
using DtmCommon;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Services;

public class DtmGrpcService : Dtm.DtmGrpcService.DtmGrpcServiceBase
{
    protected ICurrentTenant CurrentTenant { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected IDistributedEventBus DistributedEventBus { get; }
    protected IEventInfosSerializer EventInfosSerializer { get; }
    protected IActionApiTokenChecker ActionApiTokenChecker { get; }
    protected IConnectionStringHasher ConnectionStringHasher { get; }
    protected IConnectionStringResolver ConnectionStringResolver { get; }

    public DtmGrpcService(
        ICurrentTenant currentTenant,
        IServiceProvider serviceProvider,
        IDistributedEventBus distributedEventBus,
        IEventInfosSerializer eventInfosSerializer,
        IActionApiTokenChecker actionApiTokenChecker,
        IConnectionStringHasher connectionStringHasher,
        IConnectionStringResolver connectionStringResolver)
    {
        CurrentTenant = currentTenant;
        ServiceProvider = serviceProvider;
        DistributedEventBus = distributedEventBus;
        EventInfosSerializer = eventInfosSerializer;
        ActionApiTokenChecker = actionApiTokenChecker;
        ConnectionStringHasher = connectionStringHasher;
        ConnectionStringResolver = connectionStringResolver;
    }
    
    public override async Task<DtmStyleResponse> PublishEvents(DtmMsgPublishEventsRequest request, ServerCallContext context)
    {
        if (!await ActionApiTokenChecker.IsCorrectAsync(request.ActionApiToken))
        {
            return new DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }
        
        if (DistributedEventBus is not ISupportsEventBoxes supportsEventBoxes)
        {
            return new DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }
        
        var eventInfos = EventInfosSerializer.Deserialize(request.OutgoingEventInfoListToByteString);

        await supportsEventBoxes.PublishManyFromOutboxAsync(eventInfos, new OutboxConfig("DTM_Empty"));

        return new DtmStyleResponse { DtmResult = Constant.ResultSuccess };
    }
    
    public override async Task<DtmStyleResponse> QueryPrepared(QueryPreparedRequest request, ServerCallContext context)
    {
        if (!await ActionApiTokenChecker.IsCorrectAsync(request.ActionApiToken))
        {
            return new DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }
        
        var gid = context.RequestHeaders.FirstOrDefault(m => m.Key.Equals("dtm-gid"))?.Value ??
                  throw new DtmException("Cannot get dtm-gid from the gRPC request headers.");
        
        var tenantId = request.Info.TenantId.IsNullOrEmpty() ? (Guid?)null : Guid.Parse(request.Info.TenantId);

        using var changeTenant = CurrentTenant.Change(tenantId);
        
        var handlers = ServiceProvider.GetServices<IDtmQueryPreparedHandler>();

        foreach (var handler in handlers)
        {
            if (!await handler.CanHandleAsync(request.Info.DbContext))
            {
                continue;
            }
            
            return new DtmStyleResponse
            {
                DtmResult = await handler.QueryAsync(request.Info.DbContext, request.Info.HashedConnectionString, gid)
            };
        }

        throw new ApplicationException(
            $"Cannot find a DTM query prepared handler for the DbContext type {request.Info.DbContext}");
    }
}