using Volo.Abp;
using Volo.Abp.MongoDB;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.MongoDB;

public static class DtmMongoDbContextExtensions
{
    public static void ConfigureDtm(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
    }
}
