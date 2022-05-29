﻿using System.Threading.Tasks;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public abstract class DtmMsgBarrierManagerBase<TDbContextInterface> : IDtmMsgBarrierManager<TDbContextInterface>, IDtmMsgBarrierManager
    where TDbContextInterface : class
{
    public abstract Task<bool> TryInvokeInsertBarrierAsync(IDatabaseApi databaseApi, string gid);

    protected virtual bool IsValidDatabaseApi<TDatabaseApi>(IDatabaseApi databaseApi) where TDatabaseApi : IDatabaseApi
    {
        return databaseApi.GetType().IsAssignableFrom(typeof(TDatabaseApi));
    }

    public abstract Task InsertBarrierAsync(TDbContextInterface dbContext, string gid);
    
    public abstract Task<string> QueryPreparedAsync(TDbContextInterface dbContext, string gid);
}