using System;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.Abp.EventBus.Distributed.Dtm.Models;
using Microsoft.Extensions.Options;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Distributed.Dtm;

public class DtmUnitOfWork : UnitOfWork
{
    protected IDtmMessageManager DtmMessageManager { get; }

    protected DtmOutboxEventBag EventBag { get; } = new();

    public DtmUnitOfWork(
        IServiceProvider serviceProvider,
        IDtmMessageManager dtmMessageManager,
        IUnitOfWorkEventPublisher unitOfWorkEventPublisher,
        IOptions<AbpUnitOfWorkDefaultOptions> options) : base(serviceProvider, unitOfWorkEventPublisher, options)
    {
        DtmMessageManager = dtmMessageManager;
    }

    protected override async Task CommitTransactionsAsync(CancellationToken cancellationToken)
    {
        if (!EventBag.HasAnyEvent())
        {
            await base.CommitTransactionsAsync(cancellationToken);
            return;
        }
        
        OnCompleted(async () => await DtmMessageManager.SubmitAsync(EventBag, cancellationToken));

        await DtmMessageManager.PrepareAndInsertBarriersAsync(EventBag, cancellationToken);

        await base.CommitTransactionsAsync(cancellationToken);
    }

    public virtual DtmOutboxEventBag GetDtmOutboxEventBag()
    {
        return EventBag;
    }
}