using System.Threading.Tasks;

namespace EasyAbp.Abp.EventBus.Distributed.Dtm;

public interface IActionApiTokenChecker
{
    Task<bool> IsCorrectAsync(string token);
}