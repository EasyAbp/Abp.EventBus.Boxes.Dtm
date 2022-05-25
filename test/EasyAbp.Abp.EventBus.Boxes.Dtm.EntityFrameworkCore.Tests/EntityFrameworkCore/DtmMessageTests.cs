using System;
using System.Threading.Tasks;
using Dtmgrpc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.EntityFrameworkCore;

public class DtmMessageTests : DtmEntityFrameworkCoreTestBase
{
    protected override void AfterAddApplication(IServiceCollection services)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        services.AddLogging();
        services.AddDtmGrpc(x =>
        {
            x.DtmGrpcUrl = DtmGrpcProperties.DtmGrpcUrl;
            x.DtmTimeout = 10000;
        });
    }

    [Fact]
    public async Task Should_Submit_Message()
    {
        
    }
}