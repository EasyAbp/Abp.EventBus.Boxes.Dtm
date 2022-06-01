using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Boxes;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

/// <summary>
/// The DTM event inbox doesn't need a processor.
/// </summary>
public class DtmInboxProcessor : IInboxProcessor
{
    public virtual async Task StartAsync(InboxConfig inboxConfig, CancellationToken cancellationToken = new())
    {
        await Task.CompletedTask;
    }

    public virtual async Task StopAsync(CancellationToken cancellationToken = new())
    {
        await Task.CompletedTask;
    }
}