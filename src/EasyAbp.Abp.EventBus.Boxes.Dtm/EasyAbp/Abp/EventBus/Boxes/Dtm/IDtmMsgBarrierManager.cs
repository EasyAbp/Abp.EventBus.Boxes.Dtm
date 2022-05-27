using System.Threading.Tasks;
using JetBrains.Annotations;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public interface IDtmMsgBarrierManager<in TDbContextInterface> : IDtmMsgBarrierManager where TDbContextInterface : class
{
    Task InsertBarrierAsync(TDbContextInterface dbContext, [NotNull] string gid);
    
    Task<string> QueryPreparedAsync(TDbContextInterface dbContext, [NotNull] string gid);
}

public interface IDtmMsgBarrierManager
{
    /// <summary>
    /// Invokes InsertBarrierAsync method if the dbContext object has the same type as the barrier's DbContext type.
    /// </summary>
    Task<bool> TryInvokeInsertBarrierAsync(object dbContext, [NotNull] string gid);
}