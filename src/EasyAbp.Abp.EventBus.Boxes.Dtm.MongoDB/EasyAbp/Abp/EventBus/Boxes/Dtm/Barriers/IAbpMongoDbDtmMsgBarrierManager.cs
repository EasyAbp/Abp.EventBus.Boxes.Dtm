using EasyAbp.Abp.EventBus.Distributed.Dtm;
using Volo.Abp.MongoDB;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;

public interface IAbpMongoDbDtmMsgBarrierManager : IDtmMsgBarrierManager<IAbpMongoDbContext>
{
}