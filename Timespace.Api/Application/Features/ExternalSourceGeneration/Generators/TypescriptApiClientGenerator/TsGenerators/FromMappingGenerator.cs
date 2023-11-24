using System.Text;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.NewTypescriptApiClientGenerator.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.TsGenerators;

public static class FromMappingGenerator
{
    public static string GenerateFromGeneratableObject(GeneratableObject generatableObject, ITypescriptSourceBuilder sourceBuilder, List<SharedType> sharedTypes)
    {
        var blockBuilder = new StringBuilder();
        
        foreach (var generatableMember in generatableObject.Members)
        {
            if (generatableMember.Members.Count > 0)
            {
                GenerateFromGeneratableMember(generatableMember, blockBuilder, sharedTypes);
                
                sourceBuilder.AddProperty(generatableMember.Name.ToCamelCase(), generatableObject.Name + generatableMember.Name, generatableMember.MemberType.IsNullable(), generatableMember.MemberType.IsList());
            }
            else
            {
                if (Constants.MappableTypesMapping.TryGetValue(generatableMember.MemberType!.Name, out var value))
                {
                    sourceBuilder.AddProperty(generatableMember.Name, value, generatableMember.MemberType.IsNullable(), generatableMember.MemberType.IsList());
                }
            }
        }

        blockBuilder.Append(sourceBuilder);
        return blockBuilder.ToString();
    }
    
    private static void GenerateFromGeneratableMember(GeneratableMember generatableMember, StringBuilder blockBuilder, List<SharedType> sharedTypes, ITypescriptSourceBuilder? appendTo = null)
    {
        var sourceBuilder = new TypescriptInterfaceSourceBuilder().Initialize(generatableMember.Name);
        if (generatableMember.Members.Count > 0)
        {
            foreach (var generatableMemberNested in generatableMember.Members)
            {
                if (generatableMember.Members.Count > 0)
                {
                    GenerateFromGeneratableMember(generatableMemberNested, blockBuilder, sharedTypes);
                }
                else
                {
                    if (Constants.MappableTypesMapping.TryGetValue(generatableMemberNested.MemberType!.Name, out var value))
                    {
                        sourceBuilder.AddProperty(generatableMemberNested.Name, value, generatableMemberNested.MemberType.IsNullable(), generatableMemberNested.MemberType.IsList());
                    }
                }
            }
            sourceBuilder.AddProperty(generatableMember.Name, generatableMember.MemberType!.Name, generatableMember.MemberType.IsNullable(), generatableMember.MemberType.IsList());
        }
        else
        {
            sourceBuilder.AddProperty(generatableMember.Name, generatableMember.MemberType!.Name, generatableMember.MemberType.IsNullable(), generatableMember.MemberType.IsList());
        }
        
        blockBuilder.Append(sourceBuilder.Build());
    }
}
