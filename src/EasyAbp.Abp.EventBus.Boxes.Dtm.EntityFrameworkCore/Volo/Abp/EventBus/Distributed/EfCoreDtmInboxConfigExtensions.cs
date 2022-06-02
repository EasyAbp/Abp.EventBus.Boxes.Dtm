using EasyAbp.Abp.EventBus.Boxes.Dtm.Inbox;
using Volo.Abp.EntityFrameworkCore;

namespace Volo.Abp.EventBus.Distributed;

public static class EfCoreDtmInboxConfigExtensions
{
    public static void UseDbContextWithDtmInbox<TDbContext>(this InboxConfig inboxConfig)
        where TDbContext : IEfCoreDbContext
    {
        inboxConfig.ImplementationType = typeof(IDtmDbContextEventInbox<TDbContext>);
    }
}
