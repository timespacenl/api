using System.Text;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;

public static class StringExtensions
{
    public static string Repeat(this string instr, int n)
    {
        if(n <= 0)
        {
            return null;
        }
		
        if(string.IsNullOrEmpty(instr) || n == 1)
        {
            return instr;
        }
		
        return new StringBuilder(instr.Length * n)
            .Insert(0, instr, n)
            .ToString();
    }
    
    public static string ToCamelCase(this string str)
    {                    
        if(!string.IsNullOrEmpty(str) && str.Length > 1)
        {
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
        return str.ToLowerInvariant();
    }
}