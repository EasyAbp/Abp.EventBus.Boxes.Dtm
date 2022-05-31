using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public interface IDtmQueryPreparedHandler
{
    Task<bool> CanHandleAsync([NotNull] string dbContextTypeName);

    Task<bool> TryInsertBarrierAsRollbackAsync([NotNull] string dbContextTypeName,
        [NotNull] string hashedConnectionString, [NotNull] string gid);
}