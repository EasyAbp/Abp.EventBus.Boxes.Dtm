using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public abstract class DtmMsgBarrierManagerBase<TDbContextInterface> : IDtmMsgBarrierManager<TDbContextInterface>, IDtmMsgBarrierManager
    where TDbContextInterface : class
{
    protected List<Type> MatchedTypesCache { get; } = new();

    public abstract Task<bool> TryInvokeInsertBarrierAsync(object dbContext, string gid);

    protected virtual bool IsValidDbContextType(Type dbContextType)
    {
        if (dbContextType.IsIn(MatchedTypesCache))
        {
            return true;
        }
        
        var match = typeof(TDbContextInterface).IsIn(dbContextType.GetInterfaces());

        if (match)
        {
            MatchedTypesCache.Add(dbContextType);
        }

        return match;
    }

    public abstract Task InsertBarrierAsync(TDbContextInterface dbContext, string gid);
    
    public abstract Task<string> QueryPreparedAsync(TDbContextInterface dbContext, string gid);
}