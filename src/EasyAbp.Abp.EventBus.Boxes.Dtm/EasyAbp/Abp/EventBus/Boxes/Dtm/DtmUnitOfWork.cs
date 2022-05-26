﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[ExposeServices(typeof(IUnitOfWork), typeof(UnitOfWork), IncludeSelf = true)]
public class DtmUnitOfWork : UnitOfWork
{
    protected IDtmMessageManager DtmMessageManager { get; }

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
        OnCompleted(async () => await DtmMessageManager.SubmitAsync());
        
        await DtmMessageManager.PrepareAsync();
        
        await base.CommitTransactionsAsync();
    }
}