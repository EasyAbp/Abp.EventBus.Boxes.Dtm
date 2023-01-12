using System.Threading;
using System.Threading.Tasks;
using EasyAbp.Abp.EventBus.Distributed.Dtm.Models;
using JetBrains.Annotations;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Distributed.Dtm;

public interface IDtmMessageManager
{
    Task AddEventAsync(DtmOutboxEventBag eventBag, object dbContext, [NotNull] string connectionString,
        [CanBeNull] object transObj, OutgoingEventInfo eventInfo);

    Task PrepareAndInsertBarriersAsync(DtmOutboxEventBag eventBag, CancellationToken cancellationToken = default);

    Task SubmitAsync(DtmOutboxEventBag eventBag, CancellationToken cancellationToken = default);
}