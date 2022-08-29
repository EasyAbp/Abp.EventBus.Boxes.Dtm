using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace AppHttpSample;

public class SampleDtmHttpEventData
{
    public string Id { get; set; }
}
public class SampleDtmHttpEvent : IDistributedEventHandler<SampleDtmHttpEventData>,ITransientDependency
{
    private readonly ILogger<SampleDtmHttpEvent> _logger;

    public SampleDtmHttpEvent(ILogger<SampleDtmHttpEvent> logger)
    {
        _logger = logger;
    }

    public  Task HandleEventAsync(SampleDtmHttpEventData eventData)
    {
        _logger.LogInformation($"{nameof(SampleDtmHttpEvent)},Id={eventData.Id}");
        return Task.CompletedTask;
    }
}