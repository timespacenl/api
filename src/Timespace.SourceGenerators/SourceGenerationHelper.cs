namespace Timespace.SourceGenerators
{
    public static class SourceGenerationHelper
    {
        public const string Attribute = @"
namespace Timespace.SourceGenerators
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class GenerateMediatrAttribute : System.Attribute
    {
    }
}";
    }
}