using AppHttpSample.Entities;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace AppHttpSample;

public class TestDataSeed : IDataSeedContributor, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public TestDataSeed(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var testDbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await testDbContext.Database.EnsureCreatedAsync();
    }
}