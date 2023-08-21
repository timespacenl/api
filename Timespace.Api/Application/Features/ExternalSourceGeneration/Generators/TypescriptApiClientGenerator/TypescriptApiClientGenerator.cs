using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Models;
using Timespace.Api.Infrastructure.ExternalSourceGeneration;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator;

public partial class TypescriptApiClientGenerator : IExternalSourceGenerator
{
    private readonly ILogger<TypescriptApiClientGenerator> _logger;
    private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;
    private readonly Compilation _compilation;
    
    private readonly StringBuilder _generation = new();
    private readonly StringBuilder _typeGeneration = new();
    private readonly StringBuilder _enumGeneration = new();
    private readonly List<string> _generatedEnums = new();
    
    public TypescriptApiClientGenerator(ILogger<TypescriptApiClientGenerator> logger, IApiDescriptionGroupCollectionProvider apiExplorer, Compilation compilation)
    {
        _logger = logger;
        _apiExplorer = apiExplorer;
        _compilation = compilation;
    }

    public void Execute()
    {
        var apiDescriptionGroups = _apiExplorer.ApiDescriptionGroups.Items;
        foreach (var apiDescriptionGroup in apiDescriptionGroups)
        {
            var endpointDescriptions = GetEndpointInfoForApiDescriptionGroup(apiDescriptionGroup.Items, apiDescriptionGroup.GroupName);
            
            foreach (var endpointDescription in endpointDescriptions)
            {
                GenerateTypescriptCode(endpointDescription);
            }
        }

        _generation.Append(Constants.ApiClientHeaders);
        _generation.Append(_enumGeneration);
        _generation.Append(_typeGeneration);
        _logger.LogDebug("Generated typescript api client: \n{Generation}", _generation.ToString());
    }

    private void GenerateTypescriptCode(EndpointInfo endpointInfo)
    {
        var requestSchema = GenerateTypes(endpointInfo, endpointInfo.Request);
        _typeGeneration.Append(requestSchema);
        
        var responseSchema = GenerateTypes(endpointInfo, endpointInfo.Response);
        _typeGeneration.Append(responseSchema);
        
        var callingFunction = GenerateCallingFunction(endpointInfo);
        _typeGeneration.AppendLine(callingFunction);
        _typeGeneration.AppendLine();
    }
    
    private string GenerateTypes(EndpointInfo endpointInfo, ObjectInfo objectInfo)
    {
        var blockBuilder = new StringBuilder();
        
        blockBuilder = GenerateTsTypeWithChildren(endpointInfo.HandlerClassSymbol.Name.ToPascalCase(), objectInfo, blockBuilder);
        
        return blockBuilder.ToString();
    }
    
    private StringBuilder GenerateTsTypeWithChildren(string handlerClassPrefix, ObjectInfo objectInfo, StringBuilder blockBuilder)
    {
        var tsType = new TypescriptTypeBuilder(handlerClassPrefix + objectInfo.Name);

        foreach (var member in objectInfo.Members)
        {
            if (member.Children.Count == 0)
            {
                var nullable = member.OriginalSymbol.NullableAnnotation == NullableAnnotation.Annotated;
                if (member.Type.TypeKind == TypeKind.Enum)
                {
                    GenerateEnum(member.Type);
                    tsType.AddProperty(member.Name.ToCamelCase(), member.Type.Name, nullable, member.IsList);
                } 
                else 
                {
                    tsType.AddProperty(member.Name.ToCamelCase(), member.Type.GetTsType(), nullable, member.IsList);
                }
            }
            else
            {
                var newTypeName = GenerateTsTypeFromMemberinfo(handlerClassPrefix, member, blockBuilder);
                tsType.AddProperty(member.Name.ToCamelCase(), newTypeName, member.IsList);
            }
        }
        
        blockBuilder.AppendLine(tsType.Build());
        blockBuilder.AppendLine();
        return blockBuilder;
    }

    private string GenerateTsTypeFromMemberinfo(string handlerClassPrefix, MemberInfo memberInfo, StringBuilder blockBuilder)
    {
        var typeName = handlerClassPrefix + memberInfo.Type.Name;
        var tsType = new TypescriptTypeBuilder(typeName);

        foreach (var member in memberInfo.Children)
        {
            if (member.Children.Count == 0)
            {
                var nullable = member.OriginalSymbol.NullableAnnotation == NullableAnnotation.Annotated;
                if (member.Type.TypeKind == TypeKind.Enum)
                {
                    GenerateEnum(member.Type);
                    tsType.AddProperty(member.Name.ToCamelCase(), member.Type.Name, nullable, member.IsList);
                } 
                else 
                {
                    tsType.AddProperty(member.Name.ToCamelCase(), member.Type.GetTsType(), nullable, member.IsList);
                }
            }
            else
            {
                var newTypeName = GenerateTsTypeFromMemberinfo(handlerClassPrefix, member, blockBuilder);
                tsType.AddProperty(member.Name.ToCamelCase(), newTypeName, member.IsList);
            }
        }
        
        blockBuilder.AppendLine(tsType.Build());
        blockBuilder.AppendLine();
        return typeName;
    }

    private void GenerateEnum(ITypeSymbol enumPropertyType)
    {
        if(_generatedEnums.Contains(enumPropertyType.Name))
            return;
        
        var enumBuilder = new TypescriptEnumBuilder(enumPropertyType.Name);
        
        var members = enumPropertyType.GetMembers().OfType<IFieldSymbol>();
        foreach (var member in members)
        {
            if (member.ConstantValue?.GetType() == typeof(int))
            {
                enumBuilder.AddNumberEnumProperty(member.Name, member.ConstantValue.ToString()!);
            }
        }
        
        _generatedEnums.Add(enumPropertyType.Name);
        _enumGeneration.AppendLine(enumBuilder.Build());
        _enumGeneration.AppendLine();
    }

    private string GenerateCallingFunction(EndpointInfo endpointInfo)
    {
        var code = new StringBuilder();
        var url = endpointInfo.Url;
        var method = endpointInfo.HttpMethod;

        var handlerClassPrefix = endpointInfo.HandlerClassSymbol.Name.ToPascalCase();
        
        code.Append($"export const {handlerClassPrefix.ToCamelCase()} = (fetch: FetchType, request: {handlerClassPrefix}{endpointInfo.Request.Name}) => ");

        var pathParams = endpointInfo.Parameters.Where(x => x.BindingInfo?.BindingSource == BindingSource.Path);
        var queryParams = endpointInfo.Parameters.Where(x => x.BindingInfo?.BindingSource == BindingSource.Query);
        
        var queryBuilder = new UrlQueryParamBuilder();
        
        foreach (var parameter in queryParams)
        {
            queryBuilder.Add(parameter.Name.ToCamelCase(), $"${{request.{parameter.ModelMetadata.PropertyName?.ToCamelCase()}}}");
        }

        foreach (var parameter in pathParams)
        {
            url = url.Replace($"{{{parameter.Name.ToCamelCase()}}}", $"${{request.{parameter.ModelMetadata.PropertyName?.ToCamelCase()}}}");
        }
        
        url += queryBuilder.ToString();
        
        var bodyType = endpointInfo.Parameters.FirstOrDefault(x => x.BindingInfo?.BindingSource == BindingSource.Body);
        var bodyFunctionCallString = bodyType != null ? $"request.{bodyType.ModelMetadata.PropertyName?.ToCamelCase()}" : "request";
        
        var functionCall = method switch
        {
            "GET" =>
                $"genericGet<{handlerClassPrefix}{bodyType?.Type.Name ?? endpointInfo.Request.Name}, {handlerClassPrefix}{endpointInfo.Response.Name}>(fetch, `{url}`);",
            "POST" =>
                $"genericPost<{handlerClassPrefix}{bodyType?.Type.Name ?? endpointInfo.Request.Name}, {handlerClassPrefix}{endpointInfo.Response.Name}>(fetch, `{url}`, {bodyFunctionCallString});",
            "PUT" =>
                $"fetch<{handlerClassPrefix}{endpointInfo.Response.Name}>(`{url}`, {{ method: 'PUT', body: JSON.stringify(request) }});",
            "DELETE" =>
                $"fetch<{handlerClassPrefix}{endpointInfo.Response.Name}>(`{url}`, {{ method: 'DELETE', body: JSON.stringify(request) }});",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        code.Append("\n\t" + functionCall);

        return code.ToString();
    }
}