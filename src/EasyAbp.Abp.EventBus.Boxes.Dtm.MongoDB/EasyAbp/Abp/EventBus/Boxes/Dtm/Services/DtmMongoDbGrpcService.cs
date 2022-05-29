﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;
using EasyAbp.Abp.EventBus.Boxes.Dtm.MongoDB;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Services;

public class DtmMongoDbGrpcService : MongoDB.DtmMongoDbGrpcService.DtmMongoDbGrpcServiceBase
{
    private ILogger<DtmMongoDbGrpcService> Logger { get; }
    
    protected ICurrentTenant CurrentTenant { get; }
    
    protected IServiceProvider ServiceProvider { get; }

    protected IActionApiTokenChecker ActionApiTokenChecker { get; }

    protected IConnectionStringHasher ConnectionStringHasher { get; }
    
    protected IAbpMongoDbDtmMsgBarrierManager AbpMongoDbDtmMsgBarrierManager { get; }

    protected IConnectionStringResolver ConnectionStringResolver { get; }

    public DtmMongoDbGrpcService(
        ILogger<DtmMongoDbGrpcService> logger,
        ICurrentTenant currentTenant,
        IServiceProvider serviceProvider,
        IActionApiTokenChecker actionApiTokenChecker,
        IConnectionStringHasher connectionStringHasher,
        IAbpMongoDbDtmMsgBarrierManager abpMongoDbDtmMsgBarrierManager,
        IConnectionStringResolver connectionStringResolver)
    {
        Logger = logger;
        CurrentTenant = currentTenant;
        ServiceProvider = serviceProvider;
        ActionApiTokenChecker = actionApiTokenChecker;
        ConnectionStringHasher = connectionStringHasher;
        AbpMongoDbDtmMsgBarrierManager = abpMongoDbDtmMsgBarrierManager;
        ConnectionStringResolver = connectionStringResolver;
    }

    public override async Task<MongoDB.DtmStyleResponse> QueryPrepared(QueryPreparedRequest request, ServerCallContext context)
    {
        if (!await ActionApiTokenChecker.IsCorrectAsync(request.ActionApiToken))
        {
            return new MongoDB.DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }
        
        var tenantId = request.Info.TenantId.IsNullOrEmpty() ? (Guid?)null : Guid.Parse(request.Info.TenantId);

        using var changeTenant = CurrentTenant.Change(tenantId);

        var dbContextType = Type.GetType(request.Info.DbContext)!;
        var dbContextProvider =
            ServiceProvider.GetRequiredService(typeof(IMongoDbContextProvider<>).MakeGenericType(dbContextType));

        var methodInfo = dbContextProvider.GetType().GetMethod("GetDbContextAsync")!;
        var dbContext = await (Task<IAbpMongoDbContext>)methodInfo.Invoke(dbContextProvider, null)!;

        var connectionString = await ConnectionStringResolver.ResolveAsync(dbContextType);

        if (await ConnectionStringHasher.HashAsync(connectionString) != request.Info.HashedConnectionString)
        {
            return new MongoDB.DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }
        
        var gid = context.RequestHeaders.FirstOrDefault(m => m.Key.Equals("dtm-gid"))?.Value ??
                  throw new DtmException("Cannot get dtm-gid from the gRPC request headers.");

        return new MongoDB.DtmStyleResponse
        {
            DtmResult = await AbpMongoDbDtmMsgBarrierManager.QueryPreparedAsync(dbContext, gid)
        };
    }
}