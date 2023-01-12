using System;
using JetBrains.Annotations;

namespace EasyAbp.Abp.EventBus.Distributed.Dtm.Models;

public class DbConnectionLookupInfoModel
{
    [NotNull]
    public Type DbContextType { get; set; }

    public Guid? TenantId { get; set; }

    public string HashedConnectionString { get; set; }

    public DbConnectionLookupInfoModel([NotNull] Type dbContextType, Guid? tenantId, string hashedConnectionString)
    {
        DbContextType = dbContextType;
        TenantId = tenantId;
        HashedConnectionString = hashedConnectionString;
    }
}