using System.Text;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.NewTypescriptApiClientGenerator.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.TsGenerators;

public static class InterfaceGenerator
{
    public static string GenerateFromGeneratableObject(GeneratableObject generatableObject, ITypescriptSourceBuilder sourceBuilder, List<SharedType> sharedTypes)
    {
        var blockBuilder = new StringBuilder();
        
        foreach (var generatableMember in generatableObject.Members)
        {
            if (generatableMember.Members.Count > 0)
            {
                GenerateFromGeneratableMember(generatableMember, blockBuilder, generatableObject.Name, sharedTypes);
                sourceBuilder.AddProperty(generatableMember.Name.ToCamelCase(), generatableObject.Name + generatableMember.Name, false, false);
            }
            else
            {
                if (generatableMember.MemberType is not null)
                {
                    if (Constants.MappableTypesMapping.TryGetValue(generatableMember.MemberType!.Name, out var value))
                    {
                        sourceBuilder.AddProperty(generatableMember.Name.ToCamelCase(), value, generatableMember.MemberType.IsNullable(), generatableMember.MemberType.IsList());
                    }
                    else
                    {
                        sourceBuilder.AddProperty(generatableMember.Name.ToCamelCase(), generatableMember.Name, generatableMember.MemberType.IsNullable(), generatableMember.MemberType.IsList());
                    }
                }
                else
                {
                    sourceBuilder.AddProperty(generatableMember.Name.ToCamelCase(), "unknown", false, false);
                }
            }
        }

        blockBuilder.AppendLine(sourceBuilder.Build());
        return blockBuilder.ToString();
    }
    
    private static void GenerateFromGeneratableMember(GeneratableMember generatableMember, StringBuilder blockBuilder, string tsTypePrefix, List<SharedType> sharedTypes)
    {
        var sourceBuilder = new TypescriptInterfaceSourceBuilder().Initialize(tsTypePrefix + generatableMember.Name);
        if (generatableMember.Members.Count > 0)
        {
            foreach (var generatableMemberNested in generatableMember.Members)
            {
                if (generatableMemberNested.Members.Count > 0)
                {
                    GenerateFromGeneratableMember(generatableMemberNested, blockBuilder, tsTypePrefix, sharedTypes);
                    sourceBuilder.AddProperty(generatableMemberNested.Name.ToCamelCase(), generatableMemberNested.MemberType!.Name, generatableMemberNested.MemberType.IsNullable(), generatableMemberNested.MemberType.IsList());
                }
                else
                {
                    if (Constants.MappableTypesMapping.TryGetValue(generatableMemberNested.MemberType!.Name, out var value))
                    {
                        sourceBuilder.AddProperty(generatableMemberNested.Name.ToCamelCase(), value, generatableMemberNested.MemberType.IsNullable(), generatableMemberNested.MemberType.IsList());
                    }
                }
            }
        }
        else
        {
            sourceBuilder.AddProperty(generatableMember.Name, generatableMember.MemberType!.Name, generatableMember.MemberType.IsNullable(), generatableMember.MemberType.IsList());
        }
        
        blockBuilder.AppendLine(sourceBuilder.Build());
    }
}
