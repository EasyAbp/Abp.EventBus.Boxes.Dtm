using System.Threading.Tasks;
using JetBrains.Annotations;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public interface IDtmInboxBarrierManager<in TDbContextInterface> where TDbContextInterface : class
{
    Task EnsureInsertBarrierAsync(TDbContextInterface dbContext, [NotNull] string gid);

    Task<bool> ExistBarrierAsync(TDbContextInterface dbContext, [NotNull] string gid);
}