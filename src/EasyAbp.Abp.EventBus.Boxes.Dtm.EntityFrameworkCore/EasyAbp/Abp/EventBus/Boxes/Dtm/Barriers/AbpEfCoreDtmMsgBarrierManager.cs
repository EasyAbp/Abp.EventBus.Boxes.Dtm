using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;

public class AbpEfCoreDtmMsgBarrierManager : DtmMsgBarrierManagerBase<IAbpEfCoreDbContext>,
    IAbpEfCoreDtmMsgBarrierManager, ITransientDependency
{
    protected AbpDtmEventBoxesOptions Options { get; }

    private ILogger<AbpEfCoreDtmMsgBarrierManager> Logger { get; }

    public AbpEfCoreDtmMsgBarrierManager(
        IOptions<AbpDtmEventBoxesOptions> options,
        ILogger<AbpEfCoreDtmMsgBarrierManager> logger)
    {
        Options = options.Value;
        Logger = logger;
    }
    
    public override async Task InsertBarrierAsync(IAbpEfCoreDbContext dbContext, string gid)
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new DtmException("DTM barrier is for ABP transactional events.");
        }

        var affected = await InternalInsertBarrierAsync(dbContext, gid, Constant.TYPE_MSG);
        
        Logger?.LogDebug("currentAffected: {currentAffected}", affected);

        if (affected == 0)
        {
            throw new DtmDuplicatedException();
        }
    }

    public override async Task<string> QueryPreparedAsync(IAbpEfCoreDbContext dbContext, string gid)
    {
        try
        {
            await InternalInsertBarrierAsync(dbContext, gid, Constant.Barrier.MSG_BARRIER_REASON);
        }
        catch (Exception e)
        {
            Logger?.LogWarning(e, "Insert Barrier error, gid={gid}", gid);
            return e.Message;
        }

        try
        {
            var reason = await dbContext.Database.GetDbConnection().QueryFirstOrDefaultAsync<string>(
                string.Format(BarrierSqlTemplates.QueryPreparedSqlFormat, Options.BarrierTableName),
                new
                {
                    gid, branch_id = Constant.Barrier.MSG_BRANCHID, op = Constant.TYPE_MSG,
                    barrier_id = Constant.Barrier.MSG_BARRIER_ID
                });

            if (reason.Equals(Constant.Barrier.MSG_BARRIER_REASON))
            {
                return Constant.ResultFailure;
            }
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Query Prepared error, gid={gid}", gid);
            return ex.Message;
        }

        return string.Empty;
    }

    protected virtual async Task<int> InternalInsertBarrierAsync(IAbpEfCoreDbContext dbContext, string gid,
        string reason)
    {
        var tableAndValues = string.Format(BarrierSqlTemplates.DtmBarrierTableAndValueSqlFormat,
            Options.BarrierTableName);

        var special = BarrierSqlTemplates.DbProviderSpecialMapping.GetOrDefault(dbContext.Database.ProviderName);

        if (special is null)
        {
            throw new NotSupportedException(
                $"Database provider {dbContext.Database.ProviderName} is not supported by the DTM outbox!");
        }
        
        var sql = special.GetInsertIgnoreTemplate(tableAndValues, Constant.Barrier.PG_CONSTRAINT);

        sql = special.GetPlaceHoldSQL(sql);

        var affected = await dbContext.Database.GetDbConnection().ExecuteAsync(
            sql,
            new
            {
                trans_type = Constant.TYPE_MSG, gid = gid, branch_id = Constant.Barrier.MSG_BRANCHID,
                op = Constant.TYPE_MSG, barrier_id = Constant.Barrier.MSG_BARRIER_ID, reason = reason
            },
            dbContext.Database.CurrentTransaction?.GetDbTransaction());

        return affected;
    }

    public override async Task<bool> TryInvokeInsertBarrierAsync(object dbContext, string gid)
    {
        if (IsValidDbContextType(dbContext.GetType()))
        {
            return false;
        }

        await InsertBarrierAsync(dbContext as IAbpEfCoreDbContext, gid);

        return true;
    }
}