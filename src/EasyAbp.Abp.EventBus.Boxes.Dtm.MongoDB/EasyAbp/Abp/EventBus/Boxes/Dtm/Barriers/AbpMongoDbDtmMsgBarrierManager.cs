using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;
using Volo.Abp.Uow.MongoDB;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;

public class AbpMongoDbDtmMsgBarrierManager : DtmMsgBarrierManagerBase<IAbpMongoDbContext>,
    IAbpMongoDbDtmMsgBarrierManager, ITransientDependency
{
    protected AbpDtmEventBoxesOptions Options { get; }

    private ILogger<AbpMongoDbDtmMsgBarrierManager> Logger { get; }

    public AbpMongoDbDtmMsgBarrierManager(
        IOptions<AbpDtmEventBoxesOptions> options,
        ILogger<AbpMongoDbDtmMsgBarrierManager> logger)
    {
        Options = options.Value;
        Logger = logger;
    }

    public override async Task InsertBarrierAsync(IAbpMongoDbContext dbContext, string gid)
    {
        if (dbContext.SessionHandle is null)
        {
            throw new DtmException("DTM barrier is for ABP transactional events.");
        }

        try
        {
            await CheckAndInsertBarrierAsync(dbContext, gid, Constant.TYPE_MSG);
        }
        catch (Exception e)
        {
            if (e is DtmDuplicatedException)
            {
                Logger?.LogDebug("Barrier exists, gid={gid}, branchId={branchId}, op={op}, barrierId={barrierId}", gid,
                    Constant.Barrier.MSG_BRANCHID, Constant.TYPE_MSG, Constant.Barrier.MSG_BARRIER_ID);
            }

            throw;
        }
    }

    public override async Task<string> QueryPreparedAsync(IAbpMongoDbContext dbContext, string gid)
    {
        try
        {
            await CheckAndInsertBarrierAsync(dbContext, gid, Constant.Barrier.MSG_BARRIER_REASON);
        }
        catch (Exception e)
        {
            Logger?.LogWarning(e, "Insert Barrier error, gid={gid}", gid);
            return e.Message;
        }

        try
        {
            var fs = Options.BarrierTableName.Split('.');

            var barrier = dbContext.Client.GetDatabase(fs[0]).GetCollection<DtmBarrierDocument>(fs[1]);

            var filter = BuildFindFilters(gid, Constant.Barrier.MSG_BRANCHID, Constant.TYPE_MSG,
                Constant.Barrier.MSG_BARRIER_ID);
            
            var cursor = await barrier.FindAsync<DtmBarrierDocument>(filter);
            
            var res = await cursor.ToListAsync();

            if (res is { Count: > 0 } && res[0].Reason.Equals(Constant.Barrier.MSG_BARRIER_REASON))
            {
                return Constant.ResultFailure;
            }
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "MongoDB Query Prepared error, gid={gid}", gid);
            return ex.Message;
        }

        return string.Empty;
    }

    protected virtual async Task CheckAndInsertBarrierAsync(IAbpMongoDbContext dbContext, string gid,
        string reason)
    {
        var fs = Options.BarrierTableName.Split('.');
        var barrier = dbContext.Client.GetDatabase(fs[0]).GetCollection<DtmBarrierDocument>(fs[1]);

        List<DtmBarrierDocument> res;

        try
        {
            var filter = BuildFindFilters(gid, Constant.Barrier.MSG_BRANCHID, Constant.TYPE_MSG,
                Constant.Barrier.MSG_BARRIER_ID);
            
            var cursor = await barrier.FindAsync<DtmBarrierDocument>(filter);
            
            res = await cursor.ToListAsync();
        }
        catch (Exception e)
        {
            Logger?.LogDebug(e,
                "Find document exception here, gid={gid}, branchId={branchId}, op={op}, barrierId={barrierId}", gid,
                Constant.Barrier.MSG_BRANCHID, Constant.TYPE_MSG, Constant.Barrier.MSG_BARRIER_ID);

            throw;
        }

        if (res is { Count: > 0 })
        {
            throw new DtmDuplicatedException();
        }

        await barrier.InsertOneAsync(new DtmBarrierDocument
        {
            TransType = Constant.TYPE_MSG,
            GId = gid,
            BranchId = Constant.Barrier.MSG_BRANCHID,
            Op = Constant.TYPE_MSG,
            BarrierId = Constant.Barrier.MSG_BARRIER_ID,
            Reason = reason
        });
    }

    public override async Task<bool> TryInvokeInsertBarrierAsync(IDatabaseApi databaseApi, string gid)
    {
        if (IsValidDatabaseApi<MongoDbDatabaseApi>(databaseApi))
        {
            return false;
        }

        await InsertBarrierAsync(((MongoDbDatabaseApi)databaseApi).DbContext, gid);

        return true;
    }

    protected virtual FilterDefinition<DtmBarrierDocument> BuildFindFilters(string gid, string branchId, string op,
        string barrierId)
    {
        return new FilterDefinitionBuilder<DtmBarrierDocument>().And(
            Builders<DtmBarrierDocument>.Filter.Eq(x => x.GId, gid),
            Builders<DtmBarrierDocument>.Filter.Eq(x => x.BranchId, branchId),
            Builders<DtmBarrierDocument>.Filter.Eq(x => x.Op, op),
            Builders<DtmBarrierDocument>.Filter.Eq(x => x.BarrierId, barrierId)
        );
    }
}