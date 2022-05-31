using App.Shared;
using Volo.Abp.Application.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace App1.Services;

public class EventAppService : ApplicationService
{
    protected IDistributedEventBus DistributedEventBus { get; }

    public EventAppService(IDistributedEventBus distributedEventBus)
    {
        DistributedEventBus = distributedEventBus;
    }

    [UnitOfWork(false)]
    public virtual async Task PublishAsync(string message)
    {
        await DistributedEventBus.PublishAsync(new TextMessageEto(message));
    }

    [UnitOfWork(true)]
    public virtual async Task PublishInTransactionAsync(string message)
    {
        await DistributedEventBus.PublishAsync(new TextMessageEto(message));
    }
}
