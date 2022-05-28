using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class DtmUnitOfWork : UnitOfWork
{
    protected IDtmMessageManager DtmMessageManager { get; }

    protected AsyncLocalDtmOutboxEventBag AsyncLocalEventBag { get; }

    public DtmUnitOfWork(
        IServiceProvider serviceProvider,
        IDtmMessageManager dtmMessageManager,
        AsyncLocalDtmOutboxEventBag asyncLocalEventBag,
        IUnitOfWorkEventPublisher unitOfWorkEventPublisher,
        IOptions<AbpUnitOfWorkDefaultOptions> options) : base(serviceProvider, unitOfWorkEventPublisher, options)
    {
        DtmMessageManager = dtmMessageManager;
        AsyncLocalEventBag = asyncLocalEventBag;
    }

    protected override async Task CommitTransactionsAsync()
    {
        var eventBag = AsyncLocalEventBag.GetOrCreate();

        OnCompleted(async () => await DtmMessageManager.SubmitAsync(eventBag));

        await DtmMessageManager.InsertBarriersAndPrepareAsync(eventBag);

        await base.CommitTransactionsAsync();
    }
}