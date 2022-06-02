using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Inbox;

public interface IDtmDbContextEventInbox<TDbContext> : IEventInbox where TDbContext : IEfCoreDbContext
{
    
}