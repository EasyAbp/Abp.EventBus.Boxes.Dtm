using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.EntityFrameworkCore;

[ConnectionStringName(DtmDbProperties.ConnectionStringName)]
public class DtmDbContext : AbpDbContext<DtmDbContext>, IDtmDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public DtmDbContext(DbContextOptions<DtmDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureDtm();
    }
}
