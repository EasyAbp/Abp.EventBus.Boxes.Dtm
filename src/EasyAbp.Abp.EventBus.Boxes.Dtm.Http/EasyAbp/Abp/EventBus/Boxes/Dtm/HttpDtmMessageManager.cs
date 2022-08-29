using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dtmcli;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Http.EasyAbp.Abp.EventBus.Boxes.Options;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Models;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

/// <summary>
/// http调用
/// </summary>
public class HttpDtmMessageManager : IDtmMessageManager, ITransientDependency
{
    protected ICurrentTenant CurrentTenant { get; }

    protected IDtmMsgGidProvider GidProvider { get; }

    protected IDtmTransFactory DtmTransFactory { get; }

    protected IServiceProvider ServiceProvider { get; }

    protected IUnitOfWorkManager UnitOfWorkManager { get; }

    protected IEventInfosSerializer EventInfosSerializer { get; }

    protected IConnectionStringHasher ConnectionStringHasher { get; }

    protected IConnectionStringResolver ConnectionStringResolver { get; }

    protected AbpDtmHttpOptions AbpDtmHttpOptions { get; }

    public HttpDtmMessageManager(
        ICurrentTenant currentTenant,
        IDtmMsgGidProvider gidProvider,
        IDtmTransFactory dtmTransFactory,
        IServiceProvider serviceProvider,
        IUnitOfWorkManager unitOfWorkManager,
        IEventInfosSerializer eventInfosSerializer,
        IConnectionStringHasher connectionStringHasher,
        IConnectionStringResolver connectionStringResolver,
        IOptions<AbpDtmHttpOptions> abpDtmHttpOptions)
    {
        CurrentTenant = currentTenant;
        GidProvider = gidProvider;
        DtmTransFactory = dtmTransFactory;
        ServiceProvider = serviceProvider;
        UnitOfWorkManager = unitOfWorkManager;
        EventInfosSerializer = eventInfosSerializer;
        ConnectionStringHasher = connectionStringHasher;
        ConnectionStringResolver = connectionStringResolver;
        AbpDtmHttpOptions = abpDtmHttpOptions.Value;
    }

    public virtual async Task AddEventAsync(DtmOutboxEventBag eventBag, object dbContext, string connectionString,
        object transObj, OutgoingEventInfo eventInfo)
    {
        var dbContextType = dbContext.GetType();
        var hashedConnectionString = await ConnectionStringHasher.HashAsync(connectionString);

        var model = GetOrCreateDtmMessageInfoModel(eventBag, transObj, dbContextType, hashedConnectionString);

        model.EventInfos.Add(eventInfo);
    }

    public virtual async Task PrepareAndInsertBarriersAsync(DtmOutboxEventBag eventBag,
        CancellationToken cancellationToken = default)
    {
        await AddEventsPublishingActionAsync(eventBag);
        await PrepareTransMessagesAsync(eventBag, cancellationToken);

        await InsertTransMessagesBarriersAsync(eventBag, cancellationToken);
    }

    public virtual async Task SubmitAsync(DtmOutboxEventBag eventBag, CancellationToken cancellationToken = default)
    {
        if (eventBag.DefaultMessage is not null)
        {
            var message = eventBag.DefaultMessage.DtmMessage as Msg;
            await message!.Submit(cancellationToken);
        }

        foreach (var model in eventBag.TransMessages.Values)
        {
            var message = model.DtmMessage as Msg;
            await message!.Submit(cancellationToken);
        }
    }

    protected virtual IDtmMessageInfoModel GetOrCreateDtmMessageInfoModel(DtmOutboxEventBag eventBag,
        [CanBeNull] object transObj, Type dbContextType, string hashedConnectionString)
    {
        if (transObj is null)
        {
            return eventBag.DefaultMessage ?? (eventBag.DefaultMessage =
                CreateDtmMessageInfoModel(dbContextType, hashedConnectionString));
        }

        return eventBag.TransMessages.GetOrAdd(transObj,
            _ => CreateDtmMessageInfoModel(dbContextType, hashedConnectionString));
    }

    protected virtual IDtmMessageInfoModel CreateDtmMessageInfoModel(Type dbContextType, string hashedConnectionString)
    {
        var gid = GidProvider.Create();

        return new HttpDtmMessageInfoModel(gid, DtmTransFactory.NewMsg(gid),
            new DbConnectionLookupInfoModel(dbContextType, CurrentTenant.Id, hashedConnectionString),ServiceProvider.GetService<IDtmMessageBuilder>());
    }

    protected virtual async Task AddEventsPublishingActionAsync(DtmOutboxEventBag eventBag)
    {
        if ( eventBag.DefaultMessage is HttpDtmMessageInfoModel defaultMessage)
        {
            await defaultMessage.AddEventsPublishingActionAsync(AbpDtmHttpOptions, EventInfosSerializer);
        }
        foreach (var message in eventBag.TransMessages.Values.Select(model => model as HttpDtmMessageInfoModel))
        {
           await message!.AddEventsPublishingActionAsync(AbpDtmHttpOptions, EventInfosSerializer);
        }
    }

    protected virtual async Task PrepareTransMessagesAsync(DtmOutboxEventBag eventBag,
        CancellationToken cancellationToken = default)
    {
        foreach (var model in eventBag.TransMessages.Values)
        {
            var message = (model.DtmMessage as Msg)!;
            await message.Prepare(AbpDtmHttpOptions.GetQueryPreparedAddress(), cancellationToken);
        }
    }

    protected virtual async Task InsertTransMessagesBarriersAsync(DtmOutboxEventBag eventBag,
        CancellationToken cancellationToken = default)
    {
        foreach (var model in eventBag.TransMessages.Values)
        {
            var barrierManagers = ServiceProvider.GetServices<IDtmMsgBarrierManager>();

            var databaseApi = await GetDatabaseApiAsync(model.DbConnectionLookupInfo.DbContextType);

            var inserted = false;

            foreach (var barrierManager in barrierManagers)
            {
                if (await barrierManager.TryInvokeEnsureInsertBarrierAsync(databaseApi, model.Gid, cancellationToken))
                {
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
            {
                throw new AbpException(
                    $"No match DTM message barrier manager to {model.DbConnectionLookupInfo.DbContextType.Name}.");
            }
        }
    }

    protected virtual async Task<IDatabaseApi> GetDatabaseApiAsync(Type targetDbContextType)
    {
        var connectionString = await ConnectionStringResolver.ResolveAsync(targetDbContextType);

        var databaseApiKey = $"{targetDbContextType.FullName}_{connectionString}";

        var databaseApi = UnitOfWorkManager.Current.FindDatabaseApi(databaseApiKey);

        Check.NotNull(databaseApi, nameof(databaseApi));

        return databaseApi;
    }
}