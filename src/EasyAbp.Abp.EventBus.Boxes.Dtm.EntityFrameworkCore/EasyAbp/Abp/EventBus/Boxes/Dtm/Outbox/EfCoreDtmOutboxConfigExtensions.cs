using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;

public static class EfCoreDtmOutboxConfigExtensions
{
    public static void UseDbContextWithDtmOutbox<TDbContext>(this OutboxConfig outboxConfig)
        where TDbContext : IEfCoreDbContext
    {
        outboxConfig.ImplementationType = typeof(IDtmDbContextEventOutbox<TDbContext>);
    }
}
