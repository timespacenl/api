using System.Security.Cryptography;
using System.Text;

namespace Timespace.Api.Infrastructure.Services;

public static class RandomStringGenerator
{
    public static string CreateSecureRandomString(int count = 64) =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(count));
    
    public static string ToSha512(this string value)
    {
        using var sha = SHA512.Create();
    
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash  = sha.ComputeHash(bytes);
 
        return Convert.ToBase64String(hash);
    }
}