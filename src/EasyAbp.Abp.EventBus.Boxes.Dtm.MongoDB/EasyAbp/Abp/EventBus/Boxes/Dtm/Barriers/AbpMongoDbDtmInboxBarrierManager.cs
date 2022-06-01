using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MongoDB;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;

public class AbpMongoDbDtmInboxBarrierManager : IDtmInboxBarrierManager<IAbpMongoDbContext>, ITransientDependency
{
    protected AbpDtmEventBoxesOptions Options { get; }
    private ILogger<AbpMongoDbDtmInboxBarrierManager> Logger { get; }
    protected IDtmMongoDbBarrierCollectionInitializer BarrierCollectionInitializer { get; }

    public AbpMongoDbDtmInboxBarrierManager(
        IOptions<AbpDtmEventBoxesOptions> options,
        ILogger<AbpMongoDbDtmInboxBarrierManager> logger,
        IDtmMongoDbBarrierCollectionInitializer barrierCollectionInitializer)
    {
        Options = options.Value;
        Logger = logger;
        BarrierCollectionInitializer = barrierCollectionInitializer;
    }
    
    public virtual async Task EnsureInsertBarrierAsync(IAbpMongoDbContext dbContext, string gid)
    {
        if (dbContext.SessionHandle is null)
        {
            throw new AbpException("DTM barrier is for ABP transactional events.");
        }

        try
        {
            await InsertBarrierAsync(dbContext, gid, InboxBarrierProperties.Reason);
        }
        catch (Exception e)
        {
            if (e is DtmDuplicatedException)
            {
                Logger?.LogDebug("Barrier exists, gid={gid}, branchId={branchId}, op={op}, barrierId={barrierId}", gid,
                    InboxBarrierProperties.BranchId, InboxBarrierProperties.TransType, InboxBarrierProperties.BarrierId);
            }

            throw;
        }
    }
    
    public virtual async Task<bool> ExistBarrierAsync(IAbpMongoDbContext dbContext, string gid)
    {
        var mongoCollection = GetMongoCollection(dbContext);

        await BarrierCollectionInitializer.TryCreateIndexesAsync(mongoCollection);

        var filter = BuildFindFilters(gid, InboxBarrierProperties.BranchId, InboxBarrierProperties.Op,
            InboxBarrierProperties.BarrierId);
            
        var cursor = await mongoCollection.FindAsync<DtmBarrierDocument>(filter);
            
        var res = await cursor.ToListAsync();

        return res is { Count: > 0 };
    }
    
    protected virtual async Task InsertBarrierAsync(IAbpMongoDbContext dbContext, string gid,
        string reason)
    {
        var mongoCollection = GetMongoCollection(dbContext);

        await BarrierCollectionInitializer.TryCreateIndexesAsync(mongoCollection);

        try
        {
            await mongoCollection.InsertOneAsync(new DtmBarrierDocument
            {
                TransType = InboxBarrierProperties.TransType,
                GId = gid,
                BranchId = InboxBarrierProperties.BranchId,
                Op = InboxBarrierProperties.Op,
                BarrierId = InboxBarrierProperties.BarrierId,
                Reason = reason
            });
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
    
    protected virtual IMongoCollection<DtmBarrierDocument> GetMongoCollection(IAbpMongoDbContext dbContext)
    {
        var configuredTableName = Options.BarrierTableName ?? DtmBarrierProperties.DefaultBarrierCollectionName;

        var fs = configuredTableName.Split('.');

        return fs.Length == 2
            ? dbContext.Client.GetDatabase(fs[0]).GetCollection<DtmBarrierDocument>(fs[1])
            : dbContext.Database.GetCollection<DtmBarrierDocument>(configuredTableName);
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