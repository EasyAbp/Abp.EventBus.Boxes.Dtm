using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers;

public class DtmBarrierTableInitializer : IDtmBarrierTableInitializer, ISingletonDependency
{
    private ILogger<DtmBarrierTableInitializer> Logger { get; }
    private ConcurrentDictionary<string, bool> CreatedConnectionStrings { get; } = new();
    
    protected AbpDtmEventBoxesOptions Options { get; }

    public DtmBarrierTableInitializer(
        ILogger<DtmBarrierTableInitializer> logger,
        IOptions<AbpDtmEventBoxesOptions> options)
    {
        Logger = logger;
        Options = options.Value;
    }
    
    public virtual async Task TryCreateTableAsync(IEfCoreDbContext dbContext)
    {
        var connectionString = dbContext.Database.GetConnectionString();
        
        if (CreatedConnectionStrings.ContainsKey(connectionString!))
        {
            return;
        }
        
        var special = BarrierSqlTemplates.DbProviderSpecialMapping.GetOrDefault(dbContext.Database.ProviderName);

        Logger.LogInformation("DtmBarrierTableInitializer found database provider: {databaseName}",
            dbContext.Database.ProviderName);

        if (special is null)
        {
            throw new NotSupportedException(
                $"Database provider {dbContext.Database.ProviderName} is not supported by the DTM event boxes!");
        }

        var sql = special.GetCreateBarrierTableSql(Options);
        var currentTransaction = dbContext.Database.CurrentTransaction?.GetDbTransaction();

        await dbContext.Database.GetDbConnection().ExecuteAsync(sql, null, currentTransaction);

        CreatedConnectionStrings[connectionString] = true;
    }
}