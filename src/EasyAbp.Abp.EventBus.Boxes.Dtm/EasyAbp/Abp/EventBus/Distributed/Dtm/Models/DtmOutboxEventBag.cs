using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace EasyAbp.Abp.EventBus.Distributed.Dtm.Models;

public class DtmOutboxEventBag
{
    /// <summary>
    /// DTM message for non-transactional distributed events.
    /// </summary>
    [CanBeNull]
    public IDtmMessageInfoModel DefaultMessage { get; set; }

    /// <summary>
    /// DTM message for each transaction. Mapping from transaction objects to message models.
    /// </summary>
    public Dictionary<object, IDtmMessageInfoModel> TransMessages { get; } = new();

    public bool HasAnyEvent()
    {
        return DefaultMessage is not null || TransMessages.Any();
    }
}