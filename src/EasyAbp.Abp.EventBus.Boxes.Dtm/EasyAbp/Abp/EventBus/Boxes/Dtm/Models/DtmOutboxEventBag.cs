using System.Collections.Generic;
using JetBrains.Annotations;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Models;

public class DtmOutboxEventBag
{
    /// <summary>
    /// DTM message for non-transactional distributed events.
    /// </summary>
    [CanBeNull]
    public DtmMessageInfoModel DefaultMessage { get; internal set; }

    /// <summary>
    /// DTM message for each transaction. Mapping from transaction objects to message models.
    /// </summary>
    public Dictionary<object, DtmMessageInfoModel> TransMessages { get; } = new();
}