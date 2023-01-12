using System.Collections.Generic;
using System.Text;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Json;

namespace EasyAbp.Abp.EventBus.Distributed.Dtm;

public class Utf8JsonEventInfosSerializer : IEventInfosSerializer, ITransientDependency
{
    private readonly IJsonSerializer _jsonSerializer;

    public Utf8JsonEventInfosSerializer(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public virtual string Serialize(List<OutgoingEventInfo> eventInfos)
    {
        return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(_jsonSerializer.Serialize(eventInfos)));
    }

    public virtual List<OutgoingEventInfo> Deserialize(string byteString)
    {
        return _jsonSerializer.Deserialize<List<OutgoingEventInfo>>(
            Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(byteString)));
    }
}
