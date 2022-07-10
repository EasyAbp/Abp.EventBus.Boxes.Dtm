using System;
using System.Threading.Tasks;
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

    protected override async Task CommitTransactionsAsync()
    {
        if (!EventBag.HasAnyEvent())
        {
            foreach (var transaction in GetAllActiveTransactionApis())
            {
                await transaction.CommitAsync();
            }
            
            return;
        }
        
        OnCompleted(async () => await DtmMessageManager.SubmitAsync(EventBag));

        await DtmMessageManager.PrepareAndInsertBarriersAsync(EventBag);

        if (ThrowIfCommit)
        {
            throw new AbpException("bye");
        }
        
        foreach (var transaction in GetAllActiveTransactionApis())
        {
            await transaction.CommitAsync();
        }
    }
}