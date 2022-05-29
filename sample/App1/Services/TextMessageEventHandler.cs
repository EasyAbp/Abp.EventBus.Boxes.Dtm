using App.Shared;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace App1.Services;

public class TextMessageEventHandler : IDistributedEventHandler<TextMessageEto>, ITransientDependency
{
    protected ILogger<TextMessageEventHandler> Logger { get; }

    public TextMessageEventHandler(ILogger<TextMessageEventHandler> logger)
    {
        Logger = logger;
    }

    public Task HandleEventAsync(TextMessageEto eventData)
    {
        Logger.LogInformation("--------> App1 Retrieve Message: {Message}", eventData.Message);

        return Task.CompletedTask;
    }
}
