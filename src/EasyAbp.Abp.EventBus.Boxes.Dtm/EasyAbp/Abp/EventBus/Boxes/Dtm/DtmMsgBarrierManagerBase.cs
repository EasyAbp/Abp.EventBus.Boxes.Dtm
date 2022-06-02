using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public abstract class DtmMsgBarrierManagerBase<TDbContextInterface> : IDtmMsgBarrierManager<TDbContextInterface>, IDtmMsgBarrierManager
    where TDbContextInterface : class
{
    public abstract Task<bool> TryInvokeEnsureInsertBarrierAsync(IDatabaseApi databaseApi, string gid,
        CancellationToken cancellationToken = default);

    protected virtual bool IsValidDatabaseApi<TDatabaseApi>(IDatabaseApi databaseApi) where TDatabaseApi : IDatabaseApi
    {
        return databaseApi.GetType().IsAssignableTo(typeof(TDatabaseApi));
    }

    public abstract Task EnsureInsertBarrierAsync(TDbContextInterface dbContext, string gid,
        CancellationToken cancellationToken = default);
    
    public abstract Task<bool> TryInsertBarrierAsRollbackAsync(TDbContextInterface dbContext, string gid,
        CancellationToken cancellationToken = default);
}