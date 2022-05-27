using System;
using System.Linq;
using System.Threading.Tasks;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;
using EasyAbp.Abp.EventBus.Boxes.Dtm.EfCore;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Services;

public class DtmEfCoreGrpcService : EfCore.DtmEfCoreGrpcService.DtmEfCoreGrpcServiceBase
{
    private ILogger<DtmEfCoreGrpcService> Logger { get; }
    
    protected ICurrentTenant CurrentTenant { get; }
    
    protected IServiceProvider ServiceProvider { get; }

    protected IActionApiTokenChecker ActionApiTokenChecker { get; }

    protected IConnectionStringHasher ConnectionStringHasher { get; }
    
    protected IAbpEfCoreDtmMsgBarrierManager AbpEfCoreDtmMsgBarrierManager { get; }

    protected IConnectionStringResolver ConnectionStringResolver { get; }

    public DtmEfCoreGrpcService(
        ILogger<DtmEfCoreGrpcService> logger,
        ICurrentTenant currentTenant,
        IServiceProvider serviceProvider,
        IActionApiTokenChecker actionApiTokenChecker,
        IConnectionStringHasher connectionStringHasher,
        IAbpEfCoreDtmMsgBarrierManager abpEfCoreDtmMsgBarrierManager,
        IConnectionStringResolver connectionStringResolver)
    {
        Logger = logger;
        CurrentTenant = currentTenant;
        ServiceProvider = serviceProvider;
        ActionApiTokenChecker = actionApiTokenChecker;
        ConnectionStringHasher = connectionStringHasher;
        AbpEfCoreDtmMsgBarrierManager = abpEfCoreDtmMsgBarrierManager;
        ConnectionStringResolver = connectionStringResolver;
    }

    public override async Task<EfCore.DtmStyleResponse> QueryPrepared(QueryPreparedRequest request, ServerCallContext context)
    {
        if (!await ActionApiTokenChecker.IsCorrectAsync(request.ActionApiToken))
        {
            return new EfCore.DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }

        var tenantId = request.Info.TenantId.IsNullOrEmpty() ? (Guid?)null : Guid.Parse(request.Info.TenantId);

        using var changeTenant = CurrentTenant.Change(tenantId);

        var dbContextType = Type.GetType(request.Info.DbContext)!;
        var dbContextProvider =
            ServiceProvider.GetRequiredService(typeof(IDbContextProvider<>).MakeGenericType(dbContextType));

        var methodInfo = dbContextProvider.GetType().GetMethod("GetDbContextAsync")!;
        var dbContext = await ((Task<IAbpEfCoreDbContext>)methodInfo.Invoke(dbContextProvider, null))!;

        var connectionString = await ConnectionStringResolver.ResolveAsync(dbContextType);

        if (await ConnectionStringHasher.HashAsync(connectionString) != request.Info.HashedConnectionString)
        {
            return new EfCore.DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }

        var gid = context.RequestHeaders.FirstOrDefault(m => m.Key.Equals("dtm-gid"))?.Value ??
                  throw new DtmException("Cannot get dtm-gid from the gRPC request headers.");

        return new EfCore.DtmStyleResponse
        {
            DtmResult = await AbpEfCoreDtmMsgBarrierManager.QueryPreparedAsync(dbContext, gid)
        };
    }
}