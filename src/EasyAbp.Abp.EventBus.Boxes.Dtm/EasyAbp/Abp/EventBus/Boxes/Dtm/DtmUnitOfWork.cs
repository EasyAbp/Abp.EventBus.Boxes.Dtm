using System;
using System.Threading.Tasks;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Models;
using Microsoft.Extensions.Options;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

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

    protected override async Task CommitTransactionsAsync()
    {
        if (!EventBag.HasAnyEvent())
        {
            await base.CommitTransactionsAsync();
            return;
        }
        
        OnCompleted(async () => await DtmMessageManager.SubmitAsync(EventBag));

        await DtmMessageManager.PrepareAndInsertBarriersAsync(EventBag);

        await base.CommitTransactionsAsync();
    }

    public virtual DtmOutboxEventBag GetDtmOutboxEventBag()
    {
        return EventBag;
    }
}