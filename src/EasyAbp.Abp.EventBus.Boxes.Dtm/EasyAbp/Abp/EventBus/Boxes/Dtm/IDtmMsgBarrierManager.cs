using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public interface IDtmMsgBarrierManager<in TDbContextInterface> : IDtmMsgBarrierManager where TDbContextInterface : class
{
    Task EnsureInsertBarrierAsync(TDbContextInterface dbContext, [NotNull] string gid);
    
    Task<bool> TryInsertBarrierAsRollbackAsync(TDbContextInterface dbContext, [NotNull] string gid);
}

public interface IDtmMsgBarrierManager
{
    /// <summary>
    /// Invokes InsertBarrierAsync method if the <see cref="databaseApi"/> can be identified.
    /// </summary>
    Task<bool> TryInvokeEnsureInsertBarrierAsync(IDatabaseApi databaseApi, [NotNull] string gid);
}