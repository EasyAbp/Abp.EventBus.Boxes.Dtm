using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Json;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class Utf8JsonEventInfosSerializer : IEventInfosSerializer, ITransientDependency
{
    private readonly IJsonSerializer _jsonSerializer;

    public Utf8JsonEventInfosSerializer(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public virtual ByteString Serialize(List<OutgoingEventInfo> eventInfos)
    {
        return ByteString.CopyFrom(Encoding.UTF8.GetBytes(_jsonSerializer.Serialize(eventInfos)));
    }

    public virtual List<OutgoingEventInfo> Deserialize(ByteString byteString)
    {
        return _jsonSerializer.Deserialize<List<OutgoingEventInfo>>(byteString.ToStringUtf8());
    }
}
