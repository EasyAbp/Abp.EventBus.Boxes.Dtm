using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MongoDB;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;

public static class MongoDbDtmOutboxConfigExtensions
{
    public static void UseMongoDbContextWithDtmOutbox<TDbContext>(this OutboxConfig outboxConfig)
        where TDbContext : IAbpMongoDbContext
    {
        outboxConfig.ImplementationType = typeof(IDtmMongoDbContextEventOutbox<TDbContext>);
    }
}
