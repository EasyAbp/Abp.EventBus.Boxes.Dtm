using System;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.Abp.EventBus.Distributed.Dtm;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[ExposeServices(typeof(TestDtmUnitOfWork), typeof(DtmUnitOfWork), typeof(IUnitOfWork))]
public class TestDtmUnitOfWork : DtmUnitOfWork
{
    public bool ThrowIfCommit { get; set; }
    
    public TestDtmUnitOfWork(IServiceProvider serviceProvider, IDtmMessageManager dtmMessageManager,
        IUnitOfWorkEventPublisher unitOfWorkEventPublisher, IOptions<AbpUnitOfWorkDefaultOptions> options) : base(
        serviceProvider, dtmMessageManager, unitOfWorkEventPublisher, options)
    {
    }

    protected override async Task CommitTransactionsAsync(CancellationToken cancellationToken)
    {
        if (!EventBag.HasAnyEvent())
        {
            foreach (var transaction in GetAllActiveTransactionApis())
            {
                await transaction.CommitAsync(cancellationToken);
            }
            
            return;
        }
        
        OnCompleted(async () => await DtmMessageManager.SubmitAsync(EventBag, cancellationToken));

        await DtmMessageManager.PrepareAndInsertBarriersAsync(EventBag, cancellationToken);

        if (ThrowIfCommit)
        {
            throw new AbpException("bye");
        }
        
        foreach (var transaction in GetAllActiveTransactionApis())
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }
}