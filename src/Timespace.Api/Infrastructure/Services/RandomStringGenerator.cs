using System.Security.Cryptography;
using System.Text;

namespace Timespace.Api.Infrastructure.Services;

public static class RandomStringGenerator
{
    public static string CreateSecureRandomString(int count = 64) =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(count));
    
    public static string CreateUrlSafeRandomString(int count = 64) =>
        CreateSecureRandomString(count).ToUrlSafeBase64();
    
    public static string ToSha512(this string value)
    {
        using var sha = SHA512.Create();
    
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash  = sha.ComputeHash(bytes);
 
        return Convert.ToBase64String(hash);
    }
    
    private static string ToUrlSafeBase64(this string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }
}