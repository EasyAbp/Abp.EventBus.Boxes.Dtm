using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public abstract class DtmMsgBarrierManagerBase<TDbContextInterface> : IDtmMsgBarrierManager<TDbContextInterface>, IDtmMsgBarrierManager
    where TDbContextInterface : class
{
    protected List<Type> MatchedTypesCache { get; } = new();

    public abstract Task<bool> TryInvokeInsertBarrierAsync(IDatabaseApi databaseApi, string gid);

    protected virtual bool IsValidDatabaseApi<TDatabaseApi>(IDatabaseApi databaseApi) where TDatabaseApi : IDatabaseApi
    {
        var databaseApiType = databaseApi.GetType();
        
        if (databaseApiType.IsIn(MatchedTypesCache))
        {
            return true;
        }
        
        var match = databaseApiType.IsAssignableFrom(typeof(TDatabaseApi));

        if (match)
        {
            MatchedTypesCache.Add(databaseApiType);
        }

        return match;
    }

    public abstract Task InsertBarrierAsync(TDbContextInterface dbContext, string gid);
    
    public abstract Task<string> QueryPreparedAsync(TDbContextInterface dbContext, string gid);
}