using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Volo.Abp;
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
    protected IDtmMongoDbBarrierCollectionInitializer BarrierCollectionInitializer { get; }

    public AbpMongoDbDtmMsgBarrierManager(
        IOptions<AbpDtmEventBoxesOptions> options,
        ILogger<AbpMongoDbDtmMsgBarrierManager> logger,
        IDtmMongoDbBarrierCollectionInitializer barrierCollectionInitializer)
    {
        Options = options.Value;
        Logger = logger;
        BarrierCollectionInitializer = barrierCollectionInitializer;
    }

    public override async Task EnsureInsertBarrierAsync(IAbpMongoDbContext dbContext, string gid,
        CancellationToken cancellationToken = default)
    {
        if (dbContext.SessionHandle is null)
        {
            throw new AbpException("DTM barrier is for ABP transactional events.");
        }

        try
        {
            await InsertBarrierAsync(dbContext, gid, Constant.TYPE_MSG, cancellationToken);
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

    public override async Task<bool> TryInsertBarrierAsRollbackAsync(IAbpMongoDbContext dbContext, string gid,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await InsertBarrierAsync(dbContext, gid, Constant.Barrier.MSG_BARRIER_REASON, cancellationToken);
        }
        catch (Exception e)
        {
            Logger?.LogWarning(e, "Insert Barrier error, gid={gid}", gid);

            if (e is not DtmDuplicatedException)
            {
                throw;
            }
        }

        try
        {
            var mongoCollection = GetMongoCollection(dbContext);

            var filter = BuildFindFilters(gid, Constant.Barrier.MSG_BRANCHID, Constant.TYPE_MSG,
                Constant.Barrier.MSG_BARRIER_ID);

            var cursor = dbContext.SessionHandle is null
                ? await mongoCollection.FindAsync<DtmBarrierDocument>(filter, cancellationToken: cancellationToken)
                : await mongoCollection.FindAsync<DtmBarrierDocument>(dbContext.SessionHandle, filter,
                    cancellationToken: cancellationToken);
            
            var res = await cursor.ToListAsync(cancellationToken: cancellationToken);

            if (res is { Count: > 0 } && res[0].Reason.Equals(Constant.Barrier.MSG_BARRIER_REASON))
            {
                return true;    // The "rollback" inserted succeed.
            }
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "MongoDB Query Prepared error, gid={gid}", gid);
            throw;
        }

        return false;    // The "rollback" not inserted.
    }

    protected virtual IMongoCollection<DtmBarrierDocument> GetMongoCollection(IAbpMongoDbContext dbContext)
    {
        var configuredTableName = Options.BarrierTableName ?? DtmBarrierProperties.DefaultBarrierCollectionName;

        var fs = configuredTableName.Split('.');

        return fs.Length == 2
            ? dbContext.Client.GetDatabase(fs[0]).GetCollection<DtmBarrierDocument>(fs[1])
            : dbContext.Database.GetCollection<DtmBarrierDocument>(configuredTableName);
    }

    protected virtual async Task InsertBarrierAsync(IAbpMongoDbContext dbContext, string gid,
        string reason, CancellationToken cancellationToken = default)
    {
        var mongoCollection = GetMongoCollection(dbContext);

        await BarrierCollectionInitializer.TryCreateIndexesAsync(mongoCollection);

        try
        {
            var document = new DtmBarrierDocument
            {
                TransType = Constant.TYPE_MSG,
                GId = gid,
                BranchId = Constant.Barrier.MSG_BRANCHID,
                Op = Constant.TYPE_MSG,
                BarrierId = Constant.Barrier.MSG_BARRIER_ID,
                Reason = reason
            };

            if (dbContext.SessionHandle is null)
            {
                await mongoCollection.InsertOneAsync(
                    document,
                    cancellationToken: cancellationToken);
            }
            else
            {
                await mongoCollection.InsertOneAsync(
                    dbContext.SessionHandle,
                    document,
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception e)
        {
            if (e is MongoWriteException && e.Message.Contains("DuplicateKey"))
            {
                throw new DtmDuplicatedException();
            }
            
            throw;
        }
    }

    public override async Task<bool> TryInvokeEnsureInsertBarrierAsync(IDatabaseApi databaseApi, string gid,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidDatabaseApi<MongoDbDatabaseApi>(databaseApi))
        {
            return false;
        }

        await EnsureInsertBarrierAsync(((MongoDbDatabaseApi)databaseApi).DbContext, gid, cancellationToken);

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