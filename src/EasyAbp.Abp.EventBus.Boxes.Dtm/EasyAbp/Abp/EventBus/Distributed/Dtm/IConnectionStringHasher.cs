using System.Threading.Tasks;
using JetBrains.Annotations;

namespace EasyAbp.Abp.EventBus.Distributed.Dtm;

public interface IConnectionStringHasher
{
    Task<string> HashAsync([NotNull] string connectionString);
}