using System.Text;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator;

public static class ApiClientSourceBuilder<TGenerator> where TGenerator : ITypescriptSourceBuilder, new()
{
    public static string GenerateFromGeneratableObject(GeneratableObject generatableObject, ITypescriptSourceBuilder sourceBuilder, List<SharedType> sharedTypes, HashSet<TypescriptImportable> importableTypes, bool isGeneratingSharedType = false)
    {
        var blockBuilder = new StringBuilder();

        if(generatableObject.ObjectType is not null && generatableObject.ObjectType.IsNodaTimeType())
            importableTypes.Add(new(ImportType.DAYJS, null));
        
        if (generatableObject.ObjectType is not null && sharedTypes.IsSharedType(generatableObject.ObjectType) && !isGeneratingSharedType)
        {
            importableTypes.Add(new(ImportType.TYPE, generatableObject.ObjectType));
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

    private static void GenerateFromGeneratableMember(GeneratableMember parent, StringBuilder blockBuilder, HashSet<TypescriptImportable> importSet, string tsTypePrefix, List<SharedType> sharedTypes, ITypescriptSourceBuilder? appendTo = default)
    {
        var sourceBuilder = appendTo ?? new TGenerator().Initialize(tsTypePrefix + parent.Name);
        foreach (var generatableMember in parent.Members)
        {
            ProcessGeneratableMember(generatableMember, blockBuilder, importSet, tsTypePrefix, sharedTypes, sourceBuilder);
        }

        blockBuilder.AppendLine();
        blockBuilder.AppendLine(sourceBuilder.Build());
    }

    private static void ProcessGeneratableMember(GeneratableMember generatableMember, StringBuilder blockBuilder, HashSet<TypescriptImportable> importSet, string tsTypePrefix, List<SharedType> sharedTypes, ITypescriptSourceBuilder sourceBuilder)
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
        sourceBuilder.AddProperty(generatableMember, tsTypePrefix + generatableMember.Name);
    }

    private static void HandlePropertyType(GeneratableMember generatableMember, Type memberType, List<SharedType> sharedTypes, ITypescriptSourceBuilder sourceBuilder, HashSet<TypescriptImportable> importSet)
    {
        if (sharedTypes.IsSharedType(memberType) || memberType.IsEnum)
        {
            importSet.Add(new(ImportType.TYPE, memberType));
            sourceBuilder.AddProperty(generatableMember, memberType.Name);
        }
        else
        {
            if(memberType.IsNodaTimeType())
                importSet.Add(new(ImportType.DAYJS, null));
            sourceBuilder.AddProperty(generatableMember);
        }
    }
}
