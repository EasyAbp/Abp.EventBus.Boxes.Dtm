using System;
using System.Threading.Tasks;
using DtmCommon;
using EasyAbp.Abp.EventBus.Distributed.Dtm;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Services;

public class DtmGrpcService : Dtm.DtmGrpcService.DtmGrpcServiceBase
{
    protected ICurrentTenant CurrentTenant { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected IDistributedEventBus DistributedEventBus { get; }
    protected IEventInfosSerializer EventInfosSerializer { get; }
    protected IActionApiTokenChecker ActionApiTokenChecker { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }

    public DtmGrpcService(
        ICurrentTenant currentTenant,
        IServiceProvider serviceProvider,
        IDistributedEventBus distributedEventBus,
        IEventInfosSerializer eventInfosSerializer,
        IActionApiTokenChecker actionApiTokenChecker,
        IUnitOfWorkManager unitOfWorkManager)
    {
        CurrentTenant = currentTenant;
        ServiceProvider = serviceProvider;
        DistributedEventBus = distributedEventBus;
        EventInfosSerializer = eventInfosSerializer;
        ActionApiTokenChecker = actionApiTokenChecker;
        UnitOfWorkManager = unitOfWorkManager;
    }

    public override async Task<Empty> PublishEvents(DtmMsgPublishEventsRequest request, ServerCallContext context)
    {
        await CheckActionApiTokenAsync(context);

        var supportsEventBoxes = DistributedEventBus.AsSupportsEventBoxes();

        var eventInfos = EventInfosSerializer.Deserialize(request.OutgoingEventInfoListToByteString);

        await supportsEventBoxes.PublishManyFromOutboxAsync(eventInfos, new OutboxConfig("DTM_Empty"));

        return new Empty();
    }

    public override async Task<Empty> QueryPrepared(Empty request, ServerCallContext context)
    {
        await CheckActionApiTokenAsync(context);

        var gid = context.RequestHeaders.GetValue("dtm-gid") ??
                  throw new AbpException("Cannot get dtm-gid from the gRPC request headers.");

        var tenantIdString = context.RequestHeaders.GetValue(DtmRequestHeaderNames.TenantId);
        var tenantId = tenantIdString.IsNullOrWhiteSpace() ? (Guid?)null : Guid.Parse(tenantIdString!);

        using var unitOfWork = UnitOfWorkManager.Begin(true);
        using var changeTenant = CurrentTenant.Change(tenantId);

        var dbContextTypeName = context.RequestHeaders.GetValue(DtmRequestHeaderNames.DbContextType);
        var hashedConnectionString = context.RequestHeaders.GetValue(DtmRequestHeaderNames.HashedConnectionString);

        var handlers = ServiceProvider.GetServices<IDtmQueryPreparedHandler>();

        foreach (var handler in handlers)
        {
            if (!await handler.CanHandleAsync(dbContextTypeName))
            {
                continue;
            }

            if (await handler.TryInsertBarrierAsRollbackAsync(dbContextTypeName, hashedConnectionString, gid))
            {
                throw new RpcException(new Status(StatusCode.Aborted, Constant.ResultFailure));
            }

            await unitOfWork.CompleteAsync();

            return new Empty();
        }

        throw new AbpException(
            $"Cannot find a DTM query prepared handler for the DbContext type {dbContextTypeName}");
    }

    protected virtual async Task CheckActionApiTokenAsync(ServerCallContext context)
    {
        if (!await ActionApiTokenChecker.IsCorrectAsync(
                context.RequestHeaders.GetValue(DtmRequestHeaderNames.ActionApiToken)))
        {
            throw new AbpException("Incorrect ActionApiToken!");
        }
    }
}