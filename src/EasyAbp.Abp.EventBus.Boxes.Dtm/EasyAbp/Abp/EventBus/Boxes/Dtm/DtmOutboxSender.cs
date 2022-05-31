using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Boxes;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

/// <summary>
/// The DTM event outbox doesn't need a sender.
/// </summary>
public class DtmOutboxSender : IOutboxSender
{
    public virtual async Task StartAsync(OutboxConfig outboxConfig, CancellationToken cancellationToken = new())
    {
        await Task.CompletedTask;
    }

    public virtual async Task StopAsync(CancellationToken cancellationToken = new())
    {
        await Task.CompletedTask;
    }
}