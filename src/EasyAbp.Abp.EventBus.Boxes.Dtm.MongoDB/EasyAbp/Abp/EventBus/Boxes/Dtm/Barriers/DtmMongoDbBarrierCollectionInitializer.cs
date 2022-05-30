using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Volo.Abp.DependencyInjection;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;

public class DtmMongoDbBarrierCollectionInitializer : IDtmMongoDbBarrierCollectionInitializer, ISingletonDependency
{
    private ILogger<DtmMongoDbBarrierCollectionInitializer> Logger { get; }
    private ConcurrentDictionary<string, bool> CreatedServers { get; } = new();
    
    protected AbpDtmEventBoxesOptions Options { get; }

    public DtmMongoDbBarrierCollectionInitializer(
        ILogger<DtmMongoDbBarrierCollectionInitializer> logger,
        IOptions<AbpDtmEventBoxesOptions> options)
    {
        Logger = logger;
        Options = options.Value;
    }
    
    public virtual async Task TryCreateIndexesAsync(IMongoCollection<DtmBarrierDocument> mongoCollection)
    {
        var servers = mongoCollection.Database.Client.Settings.Servers.Select(x => x.ToString()).ToList();

        if (servers.All(x => CreatedServers.ContainsKey(x)))
        {
            return;
        }

        await mongoCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DtmBarrierDocument>(
                "{ gid: 1, branch_id: 1, op: 1, barrier_id: 1 }", new CreateIndexOptions
                {
                    Unique = true
                }));

        foreach (var server in servers)
        {
            CreatedServers[server] = true;
        }
    }
}