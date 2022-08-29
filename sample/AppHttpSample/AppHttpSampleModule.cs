using AppHttpSample.Entities;
using EasyAbp.Abp.EventBus.Boxes.Dtm;
using EasyAbp.Abp.EventBus.Boxes.Dtm.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;

namespace AppHttpSample;

[DependsOn(typeof(AbpAspNetCoreMvcModule))]
[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(AbpCachingModule))]

//dtm
[DependsOn(
       
        typeof(AbpEventBusBoxesDtmEntityFrameworkCoreModule),
        typeof(AbpEventBusBoxesDtmHttpModule),
        typeof(AbpEventBusRabbitMqModule)
    )
]
public class AppHttpSampleModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //
        Configure<AbpDistributedEventBusOptions>(options =>
        {
            options.Outboxes.Configure(config =>
            {
                config.UseDbContextWithDtmOutbox<TestDbContext>();
            });
            options.Inboxes.Configure(config =>
            {
                config.UseDbContextWithDtmInbox<TestDbContext>();
            });
        });
        var configuration = context.Services.GetConfiguration();
        context.Services.AddDbContext<TestDbContext>(options =>
        {
            options.UseNpgsql(context.Services.GetConfiguration().GetConnectionString("DefaultConnection"));
        });
        context.Services.AddAbpDbContext<TestDbContext>(options => { options.AddDefaultRepositories(true); });
        context.Services.AddDtmBoxes();
        context.Services.AddAbpDtmHttp(options =>
        {
            options.ActionApiToken = configuration["DTM:ActionApiToken"];//
            options.AppUrl = configuration["DTM:AppUrl"];
            options.DtmUrl = configuration["DTM:DtmUrl"];
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var dataSeeder = context.ServiceProvider.GetRequiredService<IDataSeeder>();
        dataSeeder.SeedAsync().GetAwaiter().GetResult();
        var app = context.GetApplicationBuilder();
        //send
        app.MapWhen(context => context.Request.Path.StartsWithSegments("/testevent"), (builder =>
        {
            builder.Run((async httpContext =>
            {
                var unitOfWorkManager=httpContext.RequestServices.GetRequiredService<IUnitOfWorkManager>();
                using (var unitOfWork= unitOfWorkManager.Begin())
                {
                    var db = unitOfWork.ServiceProvider.GetRequiredService<TestDbContext>();
                   await db.TestTable.AddAsync(new TestTable(Guid.NewGuid(), "test"));
                   await db.SaveChangesAsync();
                    var distributedEventBus = httpContext.RequestServices.GetRequiredService<IDistributedEventBus>();
                    await distributedEventBus.PublishAsync(
                        new SampleDtmHttpEventData
                        {
                            Id = Guid.NewGuid().ToString()
                        });
                   await unitOfWork.CompleteAsync();
                }
            }));
        }));
        //register dtm Middleware
        app.MapAbpDtmHttpService();
    }
}