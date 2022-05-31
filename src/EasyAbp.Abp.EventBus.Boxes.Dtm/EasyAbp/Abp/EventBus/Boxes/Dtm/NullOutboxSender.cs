using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Boxes;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class NullOutboxSender : IOutboxSender
{
    public async Task StartAsync(OutboxConfig outboxConfig, CancellationToken cancellationToken = new())
    {
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        await Task.CompletedTask;
    }
}