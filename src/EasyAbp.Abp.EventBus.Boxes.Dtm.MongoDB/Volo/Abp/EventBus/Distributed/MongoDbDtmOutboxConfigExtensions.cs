using EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;
using Volo.Abp.MongoDB;

namespace Volo.Abp.EventBus.Distributed;

public static class MongoDbDtmOutboxConfigExtensions
{
    public static void UseMongoDbContextWithDtmOutbox<TDbContext>(this OutboxConfig outboxConfig)
        where TDbContext : IAbpMongoDbContext
    {
        outboxConfig.ImplementationType = typeof(IDtmMongoDbContextEventOutbox<TDbContext>);
    }
}
