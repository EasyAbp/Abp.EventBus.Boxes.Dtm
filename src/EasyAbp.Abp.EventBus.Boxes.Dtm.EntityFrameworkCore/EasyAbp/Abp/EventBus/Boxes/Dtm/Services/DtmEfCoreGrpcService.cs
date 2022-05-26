using System;
using System.Threading.Tasks;
using DtmCommon;
using Dtmgrpc;
using EasyAbp.Abp.EventBus.Boxes.Dtm.EfCore;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
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

    protected IBranchBarrierFactory BranchBarrierFactory { get; }
    
    protected IActionApiTokenChecker ActionApiTokenChecker { get; }

    protected IConnectionStringHasher ConnectionStringHasher { get; }
    
    protected IConnectionStringResolver ConnectionStringResolver { get; }

    public DtmEfCoreGrpcService(
        ILogger<DtmEfCoreGrpcService> logger,
        ICurrentTenant currentTenant,
        IServiceProvider serviceProvider,
        IBranchBarrierFactory branchBarrierFactory,
        IActionApiTokenChecker actionApiTokenChecker,
        IConnectionStringHasher connectionStringHasher,
        IConnectionStringResolver connectionStringResolver)
    {
        Logger = logger;
        CurrentTenant = currentTenant;
        ServiceProvider = serviceProvider;
        BranchBarrierFactory = branchBarrierFactory;
        ActionApiTokenChecker = actionApiTokenChecker;
        ConnectionStringHasher = connectionStringHasher;
        ConnectionStringResolver = connectionStringResolver;
    }

    public override async Task<EfCore.DtmStyleResponse> QueryPrepared(QueryPreparedRequest request, ServerCallContext context)
    {
        if (!await ActionApiTokenChecker.IsCorrectAsync(request.ActionApiToken))
        {
            return new EfCore.DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }
        
        var barrier = BranchBarrierFactory.CreateBranchBarrier(context, Logger);

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

        return new EfCore.DtmStyleResponse
        {
            // Todo: different DB provider?
            DtmResult = await barrier.QueryPrepared(dbContext.Database.GetDbConnection())
        };
    }
}