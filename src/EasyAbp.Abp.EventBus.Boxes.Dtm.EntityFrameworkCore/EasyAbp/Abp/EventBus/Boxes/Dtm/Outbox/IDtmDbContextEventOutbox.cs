using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;

public interface IDtmDbContextEventOutbox<TDbContext> : IEventOutbox where TDbContext : IEfCoreDbContext
{
    
}