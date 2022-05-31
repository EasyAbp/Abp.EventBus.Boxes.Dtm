using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Models;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class EfCoreDtmQueryPreparedHandler : IDtmQueryPreparedHandler, ITransientDependency
{
    protected IServiceProvider ServiceProvider { get; }
    protected IConnectionStringHasher ConnectionStringHasher { get; }
    protected IAbpEfCoreDtmMsgBarrierManager BarrierManager { get; }
    protected IConnectionStringResolver ConnectionStringResolver { get; }

    public EfCoreDtmQueryPreparedHandler(
        IServiceProvider serviceProvider,
        IConnectionStringHasher connectionStringHasher,
        IAbpEfCoreDtmMsgBarrierManager barrierManager,
        IConnectionStringResolver connectionStringResolver)
    {
        ServiceProvider = serviceProvider;
        ConnectionStringHasher = connectionStringHasher;
        BarrierManager = barrierManager;
        ConnectionStringResolver = connectionStringResolver;
    }
    
    protected static ConcurrentDictionary<string, DbContextProviderInfo> CachedDbContextProviderInfo { get; } = new();
    
    public virtual Task<bool> CanHandleAsync(string dbContextTypeName)
    {
        return Task.FromResult(
            GetDbContextProviderInfoOrNull(dbContextTypeName) is not null
        );
    }

    public virtual async Task<bool> TryInsertBarrierAsRollbackAsync(string dbContextTypeName, string hashedConnectionString, string gid)
    {
        var providerInfo = GetDbContextProviderInfoOrNull(dbContextTypeName) ??
                           throw new ApplicationException($"Can not resolve the DbContext type {dbContextTypeName}");

        var dbContextProvider = ServiceProvider.GetRequiredService(providerInfo.DbContextProviderType);

        var dbContext =
            await ((Task<IEfCoreDbContext>)providerInfo.GetDbContextAsyncMethodInfo.Invoke(dbContextProvider, null))!;
        
        var connectionString = await ConnectionStringResolver.ResolveAsync(providerInfo.DbContextType);

        if (await ConnectionStringHasher.HashAsync(connectionString) != hashedConnectionString)
        {
            throw new ApplicationException($"Query prepared with a wrong HashedConnectionString, gid: {gid}");
        }
        
        return await BarrierManager.TryInsertBarrierAsRollbackAsync(dbContext, gid);
    }

    protected virtual DbContextProviderInfo GetDbContextProviderInfoOrNull(string dbContextTypeName)
    {
        if (CachedDbContextProviderInfo.ContainsKey(dbContextTypeName))
        {
            return CachedDbContextProviderInfo[dbContextTypeName];
        }
        
        var dbContextType = Type.GetType(dbContextTypeName)!;

        if (!dbContextType.IsAssignableFrom(typeof(IEfCoreDbContext)))
        {
            return null;
        }

        CachedDbContextProviderInfo.TryAdd(dbContextTypeName,
            new DbContextProviderInfo(
                dbContextType,
                typeof(IDbContextProvider<>).MakeGenericType(dbContextType),
                typeof(IDbContextProvider<>).MakeGenericType(dbContextType).GetMethod("GetDbContextAsync")));

        return CachedDbContextProviderInfo[dbContextTypeName];
    }
}