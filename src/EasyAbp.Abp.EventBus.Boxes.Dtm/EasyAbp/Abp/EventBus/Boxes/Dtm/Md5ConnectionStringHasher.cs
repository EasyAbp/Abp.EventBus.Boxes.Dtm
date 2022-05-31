using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public class Md5ConnectionStringHasher : IConnectionStringHasher, ITransientDependency
{
    public virtual Task<string> HashAsync(string connectionString)
    {
        return Task.FromResult(CreateMd5(connectionString));
    }
    
    private static string CreateMd5(string input)
    {
        // Use input string to calculate MD5 hash
        using var md5 = MD5.Create();
        
        var inputBytes = Encoding.ASCII.GetBytes(input);
        
        var hashBytes = md5.ComputeHash(inputBytes);
        
        var sb = new StringBuilder();
        
        foreach (var t in hashBytes)
        {
            sb.Append(t.ToString("X2"));
        }
        
        return sb.ToString();
    }
}