using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.EntityFrameworkCore;

[ConnectionStringName(DtmDbProperties.ConnectionString2Name)]
public class DtmTestDbContext2 : AbpDbContext<DtmTestDbContext2>, IDtmDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public DtmTestDbContext2(DbContextOptions<DtmTestDbContext2> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureDtmTest();
    }
}
