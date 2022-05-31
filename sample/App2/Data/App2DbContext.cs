using Volo.Abp.MongoDB;

namespace App2.Data;

public class App2DbContext : AbpMongoDbContext
{
    /* Add mongo collections here. Example:
     * public IMongoCollection<Question> Questions => Collection<Question>();
     */

    protected override void CreateModel(IMongoModelBuilder builder)
    {
        base.CreateModel(builder);

        //builder.Entity<YourEntity>(b =>
        //{
        //    //...
        //});
    }
}
