using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dtmgrpc;
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

public class GrpcDtmMessageManager : IDtmMessageManager, ITransientDependency
{
    protected ICurrentTenant CurrentTenant { get; }

    protected IDtmMsgGidProvider GidProvider { get; }

    protected IDtmTransFactory DtmTransFactory { get; }

    protected IServiceProvider ServiceProvider { get; }

    protected IUnitOfWorkManager UnitOfWorkManager { get; }

    protected IEventInfosSerializer EventInfosSerializer { get; }

    protected IConnectionStringHasher ConnectionStringHasher { get; }

    protected IConnectionStringResolver ConnectionStringResolver { get; }

    protected AbpDtmGrpcOptions AbpDtmGrpcOptions { get; }

    public GrpcDtmMessageManager(
        ICurrentTenant currentTenant,
        IDtmMsgGidProvider gidProvider,
        IDtmTransFactory dtmTransFactory,
        IServiceProvider serviceProvider,
        IUnitOfWorkManager unitOfWorkManager,
        IEventInfosSerializer eventInfosSerializer,
        IConnectionStringHasher connectionStringHasher,
        IConnectionStringResolver connectionStringResolver,
        IOptions<AbpDtmGrpcOptions> abpDtmGrpcOptions)
    {
        CurrentTenant = currentTenant;
        GidProvider = gidProvider;
        DtmTransFactory = dtmTransFactory;
        ServiceProvider = serviceProvider;
        UnitOfWorkManager = unitOfWorkManager;
        EventInfosSerializer = eventInfosSerializer;
        ConnectionStringHasher = connectionStringHasher;
        ConnectionStringResolver = connectionStringResolver;
        AbpDtmGrpcOptions = abpDtmGrpcOptions.Value;
    }

    public virtual async Task AddEventAsync(DtmOutboxEventBag eventBag, object dbContext, string connectionString,
        object transObj, OutgoingEventInfo eventInfo)
    {
        var dbContextType = dbContext.GetType();
        var hashedConnectionString = await ConnectionStringHasher.HashAsync(connectionString);

        var model = GetOrCreateDtmMessageInfoModel(eventBag, transObj, dbContextType, hashedConnectionString);

        model.EventInfos.Add(eventInfo);
    }

    public virtual async Task InsertBarriersAndPrepareAsync(DtmOutboxEventBag eventBag,
        CancellationToken cancellationToken = default)
    {
        await AddEventsPublishingActionAsync(eventBag);

        // WARNING:
        // the defaultMsg (with non-transactional events) will not invoke DTM's Prepare and not create a barrier.
        // That means when the DTM crashes, non-transactional events will never publish.
        // To avoid this problem, please KEEP USING TRANSACTION if you need write-operations.

        await PrepareTransMessagesAsync(eventBag);

        await InsertTransMessagesBarriersAsync(eventBag);
    }

    public virtual async Task SubmitAsync(DtmOutboxEventBag eventBag, CancellationToken cancellationToken = default)
    {
        if (eventBag.DefaultMessage is not null)
        {
            var message = eventBag.DefaultMessage.DtmMessage as MsgGrpc;
            await message!.Submit(cancellationToken);
        }

        foreach (var model in eventBag.TransMessages.Values)
        {
            var message = model.DtmMessage as MsgGrpc;
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

        return new DtmGrpcMessageInfoModel(gid, DtmTransFactory.NewMsgGrpc(gid),
            new DbConnectionLookupInfoModel(dbContextType, CurrentTenant.Id, hashedConnectionString));
    }

    protected virtual Task AddEventsPublishingActionAsync(DtmOutboxEventBag eventBag)
    {
        var defaultMessage = eventBag.DefaultMessage as DtmGrpcMessageInfoModel;
        defaultMessage?.AddEventsPublishingAction(AbpDtmGrpcOptions, EventInfosSerializer);

        foreach (var message in eventBag.TransMessages.Values.Select(model => model as DtmGrpcMessageInfoModel))
        {
            message!.AddEventsPublishingAction(AbpDtmGrpcOptions, EventInfosSerializer);
        }

        return Task.CompletedTask;
    }

    protected virtual async Task PrepareTransMessagesAsync(DtmOutboxEventBag eventBag)
    {
        foreach (var model in eventBag.TransMessages.Values)
        {
            var message = (model.DtmMessage as MsgGrpc)!;

            var dbContextType = $"{model.DbConnectionLookupInfo.DbContextType.FullName}, {model.DbConnectionLookupInfo.DbContextType.Assembly.GetName().Name}";

            message.SetBranchHeaders(new Dictionary<string, string>
            {
                {DtmRequestHeaderNames.ActionApiToken, AbpDtmGrpcOptions.ActionApiToken},
                {DtmRequestHeaderNames.DbContextType, dbContextType},
                {DtmRequestHeaderNames.TenantId, model.DbConnectionLookupInfo.TenantId.ToString()},
                {DtmRequestHeaderNames.HashedConnectionString, model.DbConnectionLookupInfo.HashedConnectionString},
            });

            await message.Prepare(AbpDtmGrpcOptions.GetQueryPreparedAddress());
        }
    }

    protected virtual async Task InsertTransMessagesBarriersAsync(DtmOutboxEventBag eventBag)
    {
        foreach (var model in eventBag.TransMessages.Values)
        {
            var barrierManagers = ServiceProvider.GetServices<IDtmMsgBarrierManager>();

            var databaseApi = await GetDatabaseApiAsync(model.DbConnectionLookupInfo.DbContextType);

            var inserted = false;

            foreach (var barrierManager in barrierManagers)
            {
                if (await barrierManager.TryInvokeInsertBarrierAsync(databaseApi, model.Gid))
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