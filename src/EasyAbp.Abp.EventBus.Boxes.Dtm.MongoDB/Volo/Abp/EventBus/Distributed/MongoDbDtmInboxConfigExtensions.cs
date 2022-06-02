using EasyAbp.Abp.EventBus.Boxes.Dtm.Inbox;
using Volo.Abp.MongoDB;

namespace Volo.Abp.EventBus.Distributed;

public static class MongoDbDtmInboxConfigExtensions
{
    public static void UseMongoDbContextWithDtmInbox<TDbContext>(this InboxConfig inboxConfig)
        where TDbContext : IAbpMongoDbContext
    {
        inboxConfig.ImplementationType = typeof(IDtmMongoDbContextEventInbox<TDbContext>);
    }
}
