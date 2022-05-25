using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.EntityFrameworkCore;

[ConnectionStringName(DtmDbProperties.ConnectionStringName)]
public interface IDtmDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
