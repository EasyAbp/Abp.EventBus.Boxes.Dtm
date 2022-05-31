using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Outbox;

public class DtmMongoDbContextEventOutbox<TDbContext> : IDtmMongoDbContextEventOutbox<TDbContext>
    where TDbContext : IAbpMongoDbContext
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected IMongoDbContextProvider<TDbContext> DbContextProvider { get; }
    protected AbpDtmEventBoxesOptions AbpDtmEventBoxesOptions { get; }
    protected IDtmMessageManager DtmMessageManager { get; }

    public DtmMongoDbContextEventOutbox(
        IUnitOfWorkManager unitOfWorkManager,
        IMongoDbContextProvider<TDbContext> dbContextProvider,
        IOptions<AbpDtmEventBoxesOptions> dtmOutboxOptions,
        IDtmMessageManager dtmMessageManager)
    {
        UnitOfWorkManager = unitOfWorkManager;
        DbContextProvider = dbContextProvider;
        AbpDtmEventBoxesOptions = dtmOutboxOptions.Value;
        DtmMessageManager = dtmMessageManager;
    }

    public virtual async Task EnqueueAsync(OutgoingEventInfo outgoingEvent)
    {
        var dbContext = await DbContextProvider.GetDbContextAsync();

        await DtmMessageManager.AddEventAsync(
            ((DtmUnitOfWork)UnitOfWorkManager.Current).GetDtmOutboxEventBag(),
            dbContext,
            ConnectionStringNameAttribute.GetConnStringName<TDbContext>(),
            dbContext.SessionHandle,
            outgoingEvent);
    }

    public virtual Task<List<OutgoingEventInfo>> GetWaitingEventsAsync(int maxCount,
        CancellationToken cancellationToken = new())
    {
        throw new NotSupportedException();
    }

    public virtual Task DeleteAsync(Guid id)
    {
        throw new NotSupportedException();
    }

    public async Task DeleteManyAsync(IEnumerable<Guid> ids)
    {
        throw new NotSupportedException();
    }
}