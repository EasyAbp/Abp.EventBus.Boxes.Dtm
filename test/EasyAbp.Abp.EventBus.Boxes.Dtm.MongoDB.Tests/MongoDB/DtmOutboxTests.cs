using System.Linq;
using System.Threading.Tasks;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Models;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;
using Xunit;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.MongoDB;

[Collection(MongoTestCollection.Name)]
public class DtmOutboxTests : DtmMongoDbTestBase
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected IDistributedEventBus DistributedEventBus { get; }
    protected IDtmMsgBarrierManager<IAbpMongoDbContext> DtmMsgBarrierManager { get; }

    public DtmOutboxTests()
    {
        UnitOfWorkManager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        DistributedEventBus = ServiceProvider.GetRequiredService<IDistributedEventBus>();
        DtmMsgBarrierManager = ServiceProvider.GetRequiredService<IDtmMsgBarrierManager<IAbpMongoDbContext>>();
    }

    [Fact]
    public virtual async Task Should_Set_DefaultMessage_With_Non_Trans_Uow()
    {
        using var uow = UnitOfWorkManager.Begin(new AbpUnitOfWorkOptions(false));
        
        await DistributedEventBus.PublishAsync(new TestEto());

        var dtmUow = uow as TestDtmUnitOfWork;
        dtmUow.ShouldNotBeNull();

        await uow.CompleteAsync();
        uow.IsCompleted.ShouldBeTrue();

        var bag = dtmUow.GetDtmOutboxEventBag();
        bag.HasAnyEvent().ShouldBeTrue();

        bag.TransMessages.ShouldBeEmpty();
        bag.DefaultMessage.ShouldNotBeNull();
        bag.DefaultMessage.EventInfos.Count.ShouldBe(1);
        bag.DefaultMessage.EventInfos[0].EventName.ShouldBe("test");
    }
    
    [Fact]
    public virtual async Task Should_Not_Insert_Barrier_As_Rollback_If_Trans_Succeeded()
    {
        IDtmMessageInfoModel messageInfoModel;

        using (var uow = UnitOfWorkManager.Begin(new AbpUnitOfWorkOptions(true)))
        {
            await DistributedEventBus.PublishAsync(new TestEto());
        
            var dtmUow = uow as TestDtmUnitOfWork;
            dtmUow.ShouldNotBeNull();

            await uow.CompleteAsync();
            uow.IsCompleted.ShouldBeTrue();
        
            var bag = dtmUow.GetDtmOutboxEventBag();
            bag.HasAnyEvent().ShouldBeTrue();
        
            bag.TransMessages.Count.ShouldBe(1);
            messageInfoModel = bag.TransMessages.First().Value;
        }

        await WithUnitOfWorkAsync(async () =>
        {
            var dbContextProvider = ServiceProvider.GetRequiredService<IMongoDbContextProvider<DtmTestMongoDbContext>>();
            var dbContext = await dbContextProvider.GetDbContextAsync();
            
            (await DtmMsgBarrierManager.TryInsertBarrierAsRollbackAsync(dbContext, messageInfoModel.Gid)).ShouldBe(false);
        });
    }
    
    [Fact]
    public virtual async Task Should_Insert_Barrier_As_Rollback_If_Trans_Rolled_Back()
    {
        IDtmMessageInfoModel messageInfoModel;

        using (var uow = UnitOfWorkManager.Begin(new AbpUnitOfWorkOptions(true)))
        {
            await DistributedEventBus.PublishAsync(new TestEto());
        
            var dtmUow = uow as TestDtmUnitOfWork;
            dtmUow.ShouldNotBeNull();

            dtmUow.ThrowIfCommit = true;

            var exception = await Should.ThrowAsync<AbpException>(() => uow.CompleteAsync());
            exception.Message.ShouldBe("bye");
            uow.IsCompleted.ShouldBeFalse();
        
            var bag = dtmUow.GetDtmOutboxEventBag();
            bag.HasAnyEvent().ShouldBeTrue();
        
            bag.TransMessages.Count.ShouldBe(1);
            messageInfoModel = bag.TransMessages.First().Value;
        }

        await WithUnitOfWorkAsync(async () =>
        {
            var dbContextProvider = ServiceProvider.GetRequiredService<IMongoDbContextProvider<DtmTestMongoDbContext>>();
            var dbContext = await dbContextProvider.GetDbContextAsync();
            
            (await DtmMsgBarrierManager.TryInsertBarrierAsRollbackAsync(dbContext, messageInfoModel.Gid)).ShouldBe(true);
        });
    }
    
    [Fact]
    public virtual async Task Should_Support_Multi_Database()
    {
        IDtmMessageInfoModel messageInfoModel;
        IDtmMessageInfoModel messageInfoModel2;

        using (var uow = UnitOfWorkManager.Begin(new AbpUnitOfWorkOptions(true)))
        {
            await DistributedEventBus.PublishAsync(new TestEto());
            await DistributedEventBus.PublishAsync(new Test2Eto());
        
            var dtmUow = uow as TestDtmUnitOfWork;
            dtmUow.ShouldNotBeNull();

            await uow.CompleteAsync();
            uow.IsCompleted.ShouldBeTrue();
        
            var bag = dtmUow.GetDtmOutboxEventBag();
            bag.HasAnyEvent().ShouldBeTrue();
        
            bag.TransMessages.Count.ShouldBe(2);
            messageInfoModel = bag.TransMessages
                .First(x => x.Value.DbConnectionLookupInfo.DbContextType == typeof(DtmTestMongoDbContext)).Value;
            messageInfoModel2 = bag.TransMessages
                .First(x => x.Value.DbConnectionLookupInfo.DbContextType == typeof(DtmTestMongoDbContext2)).Value;
        }

        await WithUnitOfWorkAsync(async () =>
        {
            var dbContextProvider = ServiceProvider.GetRequiredService<IMongoDbContextProvider<DtmTestMongoDbContext>>();
            var dbContext = await dbContextProvider.GetDbContextAsync();
            
            var dbContextProvider2 = ServiceProvider.GetRequiredService<IMongoDbContextProvider<DtmTestMongoDbContext2>>();
            var dbContext2 = await dbContextProvider2.GetDbContextAsync();
            
            (await DtmMsgBarrierManager.TryInsertBarrierAsRollbackAsync(dbContext, messageInfoModel.Gid)).ShouldBe(false);
            (await DtmMsgBarrierManager.TryInsertBarrierAsRollbackAsync(dbContext2, messageInfoModel2.Gid)).ShouldBe(false);
        });
    }
}