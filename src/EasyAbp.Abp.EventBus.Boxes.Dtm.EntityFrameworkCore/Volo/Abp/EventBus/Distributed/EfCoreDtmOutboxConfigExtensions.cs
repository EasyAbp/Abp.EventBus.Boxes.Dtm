using EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;
using Volo.Abp.EntityFrameworkCore;

namespace Volo.Abp.EventBus.Distributed;

public static class EfCoreDtmOutboxConfigExtensions
{
    public static void UseDbContextWithDtmOutbox<TDbContext>(this OutboxConfig outboxConfig)
        where TDbContext : IEfCoreDbContext
    {
        outboxConfig.ImplementationType = typeof(IDtmDbContextEventOutbox<TDbContext>);
    }
}
