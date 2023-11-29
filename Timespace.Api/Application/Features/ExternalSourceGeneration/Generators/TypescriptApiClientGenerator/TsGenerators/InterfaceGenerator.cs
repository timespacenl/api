using System.Text;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.TsGenerators;

public static class InterfaceGenerator
{
    public static string GenerateFromGeneratableObject(GeneratableObject generatableObject, ITypescriptSourceBuilder sourceBuilder, List<SharedType> sharedTypes, HashSet<Type> importableTypes)
    {
        var blockBuilder = new StringBuilder();

        if (generatableObject.ObjectType is not null && sharedTypes.IsSharedType(generatableObject.ObjectType))
        {
            importableTypes.Add(generatableObject.ObjectType);
        }
        else
        {
            foreach (var generatableMember in generatableObject.Members)
            {
                ProcessGeneratableMember(generatableMember, blockBuilder, importableTypes, generatableObject.Name, sharedTypes, sourceBuilder);
            }
            
            blockBuilder.AppendLine();
            blockBuilder.AppendLine(sourceBuilder.Build());
        }
        
        return blockBuilder.ToString();
    }

    private static void GenerateFromGeneratableMember(GeneratableMember parent, StringBuilder blockBuilder, HashSet<Type> importSet, string tsTypePrefix, List<SharedType> sharedTypes, ITypescriptSourceBuilder? appendTo = null)
    {
        var sourceBuilder = appendTo ?? new TypescriptInterfaceSourceBuilder().Initialize(tsTypePrefix + parent.Name);
        foreach (var generatableMember in parent.Members)
        {
            ProcessGeneratableMember(generatableMember, blockBuilder, importSet, tsTypePrefix, sharedTypes, sourceBuilder);
        }

        blockBuilder.AppendLine();
        blockBuilder.AppendLine(sourceBuilder.Build());
    }

    private static void ProcessGeneratableMember(GeneratableMember generatableMember, StringBuilder blockBuilder, HashSet<Type> importSet, string tsTypePrefix, List<SharedType> sharedTypes, ITypescriptSourceBuilder sourceBuilder)
    {
        var memberType = generatableMember.MemberType;

        if (memberType is null || (generatableMember.Members.Count > 0 && !sharedTypes.IsSharedType(memberType) && !memberType.IsEnum))
        {
            GenerateFromGeneratableMember(generatableMember, blockBuilder, importSet, tsTypePrefix, sharedTypes);
            AddPropertyToSourceBuilder(sourceBuilder, generatableMember, tsTypePrefix);
        }
        else
        {
            HandlePropertyType(generatableMember, memberType, sharedTypes, sourceBuilder, importSet);
        }
    }

    private static void AddPropertyToSourceBuilder(ITypescriptSourceBuilder sourceBuilder, GeneratableMember generatableMember, string tsTypePrefix)
    {
        sourceBuilder.AddProperty(generatableMember.Name.ToCamelCase(), tsTypePrefix + generatableMember.Name, generatableMember.IsNullable, generatableMember.IsList);
    }

    private static void HandlePropertyType(GeneratableMember generatableMember, Type memberType, List<SharedType> sharedTypes, ITypescriptSourceBuilder sourceBuilder, HashSet<Type> importSet)
    {
        if (sharedTypes.IsSharedType(memberType) || memberType.IsEnum)
        {
            importSet.Add(memberType);
            sourceBuilder.AddProperty(generatableMember.Name.ToCamelCase(), memberType.Name, generatableMember.IsNullable, generatableMember.IsList);
        }
        else
        {
            if (Constants.MappableTypesMapping.TryGetValue(memberType.Name, out var tsType))
            {
                sourceBuilder.AddProperty(generatableMember.Name.ToCamelCase(), tsType, generatableMember.IsNullable, generatableMember.IsList);
            }
            else
            {
                sourceBuilder.AddProperty(generatableMember.Name.ToCamelCase(), "unknown", generatableMember.IsNullable, generatableMember.IsList);
            }
        }
    }
}
