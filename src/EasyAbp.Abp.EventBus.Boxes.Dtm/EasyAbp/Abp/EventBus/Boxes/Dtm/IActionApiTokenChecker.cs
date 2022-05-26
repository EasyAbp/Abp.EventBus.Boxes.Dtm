using System.Threading.Tasks;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public interface IActionApiTokenChecker
{
    Task<bool> IsCorrectAsync(string token);
}