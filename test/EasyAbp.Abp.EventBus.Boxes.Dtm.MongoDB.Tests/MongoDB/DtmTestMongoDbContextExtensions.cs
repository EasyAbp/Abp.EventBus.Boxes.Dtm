using Volo.Abp;
using Volo.Abp.MongoDB;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.MongoDB;

public static class DtmTestMongoDbContextExtensions
{
    public static void ConfigureDtmTest(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
    }
}
