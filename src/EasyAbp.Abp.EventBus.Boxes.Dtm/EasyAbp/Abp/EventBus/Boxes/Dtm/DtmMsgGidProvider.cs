using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class DtmMsgGidProvider : IDtmMsgGidProvider, ITransientDependency
{
    private readonly IGuidGenerator _guidGenerator;

    public DtmMsgGidProvider(IGuidGenerator guidGenerator)
    {
        _guidGenerator = guidGenerator;
    }
    
    public string Create()
    {
        return _guidGenerator.Create().ToString();
    }
}