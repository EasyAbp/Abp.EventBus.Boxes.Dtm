using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MongoDB;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Inbox;

public interface IDtmMongoDbContextEventInbox<TDbContext> : IEventInbox where TDbContext : IAbpMongoDbContext
{
    
}