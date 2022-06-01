using System;
using System.Threading.Tasks;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Xunit;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.MongoDB;

[Collection(MongoTestCollection.Name)]
public class DtmInboxTests : DtmMongoDbTestBase
{
    private IGuidGenerator GuidGenerator { get; }
    
    public DtmInboxTests()
    {
        GuidGenerator = ServiceProvider.GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task Should_Not_Process_Duplicate_Events()
    {
        var e1 = new IncomingEventInfo(GuidGenerator.Create(), DtmTestConsts.MessageId1, "test", null, DateTime.Now);
        var e2 = new IncomingEventInfo(GuidGenerator.Create(), DtmTestConsts.MessageId1, "test", null, DateTime.Now);

        var inbox = ServiceProvider.GetRequiredService<IDtmMongoDbContextEventInbox<DtmTestMongoDbContext>>();

        await Should.NotThrowAsync(() => inbox.EnqueueAsync(e1));

        await Should.ThrowAsync<DtmDuplicatedException>(() => inbox.EnqueueAsync(e2));
    }
}