using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.TsGenerators;

public static class TsInterfaceGenerationExtensions
{
    public static string GenerateInterfacesFromApiParameterDescriptions(this List<ApiParameterDescription> apiParameterDescriptions, string typeName, StringBuilder blockBuilder)
    {
        var tsType = new TypescriptTypeBuilder(typeName);
        
        return blockBuilder.ToString();
    }

    public static void GenerateInterfacesFromType(this Type type, StringBuilder blockBuilder)
    {
        
    }
}