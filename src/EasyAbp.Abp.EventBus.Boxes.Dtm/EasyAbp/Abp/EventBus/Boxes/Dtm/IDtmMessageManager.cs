using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public interface IDtmMessageManager
{
    Task AddEventAsync(object dbContext, [CanBeNull] DbTransaction dbTransaction, OutgoingEventInfo eventInfo);

    Task PrepareAsync(CancellationToken cancellationToken = default);

    Task SubmitAsync(CancellationToken cancellationToken = default);
}