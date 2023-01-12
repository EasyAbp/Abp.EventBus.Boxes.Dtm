using EasyAbp.Abp.EventBus.Distributed.Dtm;
using Volo.Abp.EntityFrameworkCore;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;

public interface IAbpEfCoreDtmMsgBarrierManager : IDtmMsgBarrierManager<IEfCoreDbContext>
{
}