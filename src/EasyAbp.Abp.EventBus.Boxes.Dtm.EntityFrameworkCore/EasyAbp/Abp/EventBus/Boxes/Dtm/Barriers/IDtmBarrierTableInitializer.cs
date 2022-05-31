using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;

public interface IDtmBarrierTableInitializer
{
    Task TryCreateTableAsync(IEfCoreDbContext dbContext);
}