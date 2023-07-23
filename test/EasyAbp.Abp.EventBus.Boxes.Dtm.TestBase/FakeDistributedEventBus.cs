using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class FakeDistributedEventBus : DistributedEventBusBase, ITransientDependency
{
    public FakeDistributedEventBus(IServiceScopeFactory serviceScopeFactory, ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager, IOptions<AbpDistributedEventBusOptions> abpDistributedEventBusOptions,
        IGuidGenerator guidGenerator, IClock clock, IEventHandlerInvoker eventHandlerInvoker,
        ILocalEventBus localEventBus) : base(serviceScopeFactory, currentTenant, unitOfWorkManager,
        abpDistributedEventBusOptions, guidGenerator, clock, eventHandlerInvoker, localEventBus)
    {
    }

    public override IDisposable Subscribe(Type eventType, IEventHandlerFactory factory)
    {
        throw new NotSupportedException();
    }

    public override void Unsubscribe<TEvent>(Func<TEvent, Task> action)
    {
        throw new NotSupportedException();
    }

    public override void Unsubscribe(Type eventType, IEventHandler handler)
    {
        throw new NotSupportedException();
    }

    public override void Unsubscribe(Type eventType, IEventHandlerFactory factory)
    {
        throw new NotSupportedException();
    }

    public override void UnsubscribeAll(Type eventType)
    {
        throw new NotSupportedException();
    }

    protected override async Task PublishToEventBusAsync(Type eventType, object eventData)
    {
        await Task.CompletedTask;
    }

    protected override void AddToUnitOfWork(IUnitOfWork unitOfWork, UnitOfWorkEventRecord eventRecord)
    {
        unitOfWork.AddOrReplaceDistributedEvent(eventRecord);
    }

    protected override IEnumerable<EventTypeWithEventHandlerFactories> GetHandlerFactories(Type eventType)
    {
        throw new NotSupportedException();
    }

    public override async Task PublishFromOutboxAsync(OutgoingEventInfo outgoingEvent, OutboxConfig outboxConfig)
    {
        await Task.CompletedTask;
    }

    public override async Task PublishManyFromOutboxAsync(IEnumerable<OutgoingEventInfo> outgoingEvents,
        OutboxConfig outboxConfig)
    {
        await Task.CompletedTask;
    }

    public override async Task ProcessFromInboxAsync(IncomingEventInfo incomingEvent, InboxConfig inboxConfig)
    {
        await Task.CompletedTask;
    }

    protected override byte[] Serialize(object eventData)
    {
        return null;
    }
}