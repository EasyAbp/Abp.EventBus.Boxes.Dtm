using Volo.Abp.MongoDB.DistributedEvents;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;

public interface IDtmMongoDbContextEventOutbox<TDbContext> : IMongoDbContextEventOutbox<TDbContext>
    where TDbContext : IHasEventOutbox
{
    
}