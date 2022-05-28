using System.Threading;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Models;
using Volo.Abp.DependencyInjection;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class AsyncLocalDtmOutboxEventBag : ISingletonDependency
{
    private readonly AsyncLocal<DtmOutboxEventBag> _value = new();

    public DtmOutboxEventBag Value
    {
        get => _value.Value;
        private set => _value.Value = value;
    }

    public DtmOutboxEventBag GetOrCreate()
    {
        return Value ??= new DtmOutboxEventBag();
    }
}