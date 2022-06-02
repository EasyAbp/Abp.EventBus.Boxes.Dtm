using System;
using Volo.Abp.EventBus;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[EventName("test2")]
[Serializable]
public class Test2Eto
{
    public string Content { get; set; }
}