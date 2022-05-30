using App.Shared;
using Volo.Abp.Application.Services;
using Volo.Abp.EventBus.Distributed;

namespace App1.Services;

public class EventAppService : ApplicationService
{
    protected IDistributedEventBus DistributedEventBus { get; }

    public EventAppService(IDistributedEventBus distributedEventBus)
    {
        DistributedEventBus = distributedEventBus;
    }

    public virtual async Task PublishAsync(string message)
    {
        await DistributedEventBus.PublishAsync(new TextMessageEto(message));
    }
}
