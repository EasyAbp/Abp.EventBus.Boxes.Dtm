using System.Threading.Tasks;
using DtmCommon;
using Grpc.Core;
using Volo.Abp.EventBus.Distributed;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Services;

public class DtmGrpcService : Dtm.DtmGrpcService.DtmGrpcServiceBase
{
    protected IDistributedEventBus DistributedEventBus { get; }
    
    protected IEventInfosSerializer EventInfosSerializer { get; }
    
    protected IActionApiTokenChecker ActionApiTokenChecker { get; }

    public DtmGrpcService(
        IDistributedEventBus distributedEventBus,
        IEventInfosSerializer eventInfosSerializer,
        IActionApiTokenChecker actionApiTokenChecker)
    {
        DistributedEventBus = distributedEventBus;
        EventInfosSerializer = eventInfosSerializer;
        ActionApiTokenChecker = actionApiTokenChecker;
    }
    
    public override async Task<DtmStyleResponse> PublishEvents(DtmMsgPublishEventsRequest request, ServerCallContext context)
    {
        if (!await ActionApiTokenChecker.IsCorrectAsync(request.ActionApiToken))
        {
            return new DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }
        
        if (DistributedEventBus is not ISupportsEventBoxes supportsEventBoxes)
        {
            return new DtmStyleResponse { DtmResult = Constant.ResultFailure };
        }
        
        var eventInfos = EventInfosSerializer.Deserialize(request.OutgoingEventInfoListToByteString);

        await supportsEventBoxes.PublishManyFromOutboxAsync(eventInfos, new OutboxConfig("DTM_Empty"));

        return new DtmStyleResponse { DtmResult = Constant.ResultSuccess };
    }
}