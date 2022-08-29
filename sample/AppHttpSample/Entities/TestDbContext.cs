using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace AppHttpSample.Entities;

public class TestDbContext : AbpDbContext<TestDbContext>
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public virtual DbSet<TestTable> TestTable { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // modelBuilder.Entity<ExtraPropertyDictionary>().HasNoKey();
        modelBuilder.Entity<TestTable>(b =>
        {
            b.ToTable("test_table");

            //Configure the base properties
            b.ConfigureByConvention();
            //Configure other properties (if you are using the fluent API)
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);

        });
    }
}