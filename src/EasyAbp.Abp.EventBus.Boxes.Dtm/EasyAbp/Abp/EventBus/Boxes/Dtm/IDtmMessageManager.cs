using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public interface IDtmMessageManager
{
    Task AddEventAsync(object dbContext, [NotNull] string connectionString, [CanBeNull] object transObj,
        OutgoingEventInfo eventInfo);

    Task InsertBarriersAndPrepareAsync(CancellationToken cancellationToken = default);

    Task SubmitAsync(CancellationToken cancellationToken = default);
}