using Volo.Abp.EntityFrameworkCore.DistributedEvents;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;

public interface IDtmDbContextEventOutbox<TDbContext> : IDbContextEventOutbox<TDbContext>
    where TDbContext : IHasEventOutbox
{
    
}