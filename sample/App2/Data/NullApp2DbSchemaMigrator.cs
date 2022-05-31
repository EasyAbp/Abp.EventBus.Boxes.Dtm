using Volo.Abp.DependencyInjection;

namespace App2.Data;

public class NullApp2DbSchemaMigrator : ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
