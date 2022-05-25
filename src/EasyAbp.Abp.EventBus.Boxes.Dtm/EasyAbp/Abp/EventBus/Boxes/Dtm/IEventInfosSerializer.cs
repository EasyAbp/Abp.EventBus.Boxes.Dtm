using System.Collections.Generic;
using Google.Protobuf;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public interface IEventInfosSerializer
{
    ByteString Serialize(List<OutgoingEventInfo> eventInfos);

    List<OutgoingEventInfo> Deserialize(ByteString byteString);
}
