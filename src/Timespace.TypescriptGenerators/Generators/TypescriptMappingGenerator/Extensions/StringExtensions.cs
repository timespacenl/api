using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Extensions;

public static class StringExtensions
{
	public static string Repeat(this string instr, int n)
	{
		if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "n must be greater than 0");

		return string.IsNullOrEmpty(instr) || n == 1
			? instr
			: new StringBuilder(instr.Length * n)
			.Insert(0, instr, n)
			.ToString();
	}

	[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "This is a string extension method, not a general purpose method")]
	public static string ToCamelCase(this string str)
	{
		return !string.IsNullOrEmpty(str) && str.Length > 1 ? char.ToLowerInvariant(str[0]) + str[1..] : str.ToLowerInvariant();
	}

	public static string ToPascalCase(this string str)
	{
		return !string.IsNullOrEmpty(str) && str.Length > 1 ? char.ToUpperInvariant(str[0]) + str[1..] : str.ToUpperInvariant();
	}
}
