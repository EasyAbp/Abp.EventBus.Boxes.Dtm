using System;
using System.Data.Common;
using System.Threading.Tasks;
using DtmCommon;
using Dtmgrpc;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Services;

public class DtmGrpcService : Dtm.DtmGrpcService.DtmGrpcServiceBase
{
    private ILogger<DtmGrpcService> Logger { get; }
    
    protected ICurrentTenant CurrentTenant { get; }

    protected DtmOutboxOptions DtmOutboxOptions { get; }
    
    protected IDistributedEventBus DistributedEventBus { get; }
    
    protected IBranchBarrierFactory BranchBarrierFactory { get; }
    
    protected IEventInfosSerializer EventInfosSerializer { get; }
    
    protected IConnectionStringHasher ConnectionStringHasher { get; }

    protected IConnectionStringResolver ConnectionStringResolver { get; }

    public DtmGrpcService(
        ILogger<DtmGrpcService> logger,
        ICurrentTenant currentTenant,
        IOptions<DtmOutboxOptions> dtmOptions,
        IDistributedEventBus distributedEventBus,
        IBranchBarrierFactory branchBarrierFactory,
        IEventInfosSerializer eventInfosSerializer,
        IConnectionStringHasher connectionStringHasher,
        IConnectionStringResolver connectionStringResolver)
    {
        Logger = logger;
        CurrentTenant = currentTenant;
        DtmOutboxOptions = dtmOptions.Value;
        DistributedEventBus = distributedEventBus;
        BranchBarrierFactory = branchBarrierFactory;
        EventInfosSerializer = eventInfosSerializer;
        ConnectionStringHasher = connectionStringHasher;
        ConnectionStringResolver = connectionStringResolver;
    }
    
    public override async Task<DtmStyleResponse> PublishEvents(DtmMsgPublishEventsRequest request, ServerCallContext context)
    {
        if (!await IsActionApiTokenCorrectAsync(request.ActionApiToken))
        {
            return new DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }
        
        if (DistributedEventBus is not ISupportsEventBoxes supportsEventBoxes)
        {
            return new DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }
        
        var eventInfos = EventInfosSerializer.Deserialize(request.OutgoingEventInfoListToByteString);

        await supportsEventBoxes.PublishManyFromOutboxAsync(eventInfos, new OutboxConfig("Default"));

        return new DtmStyleResponse { DtmResult = Constant.ResultSuccess };
    }

    public override async Task<DtmStyleResponse> QueryPrepared(QueryPreparedRequest request, ServerCallContext context)
    {
        if (!await IsActionApiTokenCorrectAsync(request.ActionApiToken))
        {
            return new DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }
        
        // Todo: should design a new factory to resolve barriers for EF Core and MongoDB.
        var barrier = BranchBarrierFactory.CreateBranchBarrier(context, Logger);

        var tenantId = request.Info.TenantId.IsNullOrEmpty() ? (Guid?)null : Guid.Parse(request.Info.TenantId);

        using var changeTenant = CurrentTenant.Change(tenantId);
        
        var connectionString = await ConnectionStringResolver.ResolveAsync(request.Info.ConnectionStringName);

        if (await ConnectionStringHasher.HashAsync(connectionString) != request.Info.ConnectionStringName)
        {
            return new DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }

        // Todo: how to get a DbConnection?
        DbConnection db = null;
        
        return new DtmStyleResponse
        {
            DtmResult = await barrier.QueryPrepared(db)
        };
    }
    
    protected virtual Task<bool> IsActionApiTokenCorrectAsync(string requestActionApiToken)
    {
        return Task.FromResult(requestActionApiToken == DtmOutboxOptions.ActionApiToken);
    }
}