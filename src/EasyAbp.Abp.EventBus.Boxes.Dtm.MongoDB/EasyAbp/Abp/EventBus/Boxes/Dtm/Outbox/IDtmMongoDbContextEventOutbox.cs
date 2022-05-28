using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MongoDB;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;

public interface IDtmMongoDbContextEventOutbox<TDbContext> : IEventOutbox where TDbContext : IAbpMongoDbContext
{
    
}