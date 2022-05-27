using Volo.Abp.MongoDB;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;

public interface IAbpMongoDbDtmMsgBarrierManager : IDtmMsgBarrierManager<IAbpMongoDbContext>
{
}