using System;
using Volo.Abp.EventBus;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[EventName("test")]
[Serializable]
public class TestEto
{
    public string Content { get; set; }
}