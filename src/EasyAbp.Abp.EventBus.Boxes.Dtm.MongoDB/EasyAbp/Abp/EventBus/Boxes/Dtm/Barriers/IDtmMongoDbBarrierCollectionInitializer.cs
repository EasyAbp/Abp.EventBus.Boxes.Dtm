using System.Threading.Tasks;
using MongoDB.Driver;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;

public interface IDtmMongoDbBarrierCollectionInitializer
{
    Task TryCreateIndexesAsync(IMongoCollection<DtmBarrierDocument> mongoCollection);
}