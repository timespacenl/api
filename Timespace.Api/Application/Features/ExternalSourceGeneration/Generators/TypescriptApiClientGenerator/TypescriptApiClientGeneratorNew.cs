using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.NewTypescriptApiClientGenerator.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.TsGenerators;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;
using Timespace.Api.Infrastructure.Configuration;
using Timespace.Api.Infrastructure.ExternalSourceGeneration;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator;

public class TypescriptApiClientGeneratorNew : IExternalSourceGenerator
{
    private readonly ILogger<TypescriptApiClientGeneratorNew> _logger;
    private readonly Compilation _compilation;
    private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;
    private readonly StringBuilder _generation = new();
    private readonly StringBuilder _typeGeneration = new();
    private readonly StringBuilder _enumGeneration = new();
    private readonly List<string> _generatedEnums = new();
    private readonly ExternalSourceGenerationSettings _options;
    private readonly List<GeneratableEndpoint> _generatableEndpoints = new();
    private HashSet<Type> _seenTypes = new();

    public TypescriptApiClientGeneratorNew(Compilation compilation, IApiDescriptionGroupCollectionProvider apiExplorer, ILogger<TypescriptApiClientGeneratorNew> logger, IOptions<ExternalSourceGenerationSettings> options)
    {
        _compilation = compilation;
        _apiExplorer = apiExplorer;
        _logger = logger;
        _options = options.Value;
    }

    public void Execute()
    {
        var apiDescriptionGroups = _apiExplorer.ApiDescriptionGroups.Items;
        foreach (var apiDescriptionGroup in apiDescriptionGroups)
        {
            foreach (var apiDescription in apiDescriptionGroup.Items)
            {
                var generatableEndpoint = TransformApiDescription(apiDescription);
                
                if (generatableEndpoint is null)
                    continue;
                
                _generatableEndpoints.Add(generatableEndpoint);
                // GenerateFromEndpoint(generatableEndpoint);
            }
        }
        
        var sharedTypes = GetSharedTypes(_generatableEndpoints);
        GenerateSharedTypes(sharedTypes);
        
        _generation.AppendLine(Constants.ApiClientHeaders);
        _generation.AppendLine(_enumGeneration.ToString());
        _generation.AppendLine(_typeGeneration.ToString());
        
        File.WriteAllText(_options.TypescriptGenerator.GenerationPath + '\\' + _options.TypescriptGenerator.GenerationFileName + ".ts", _generation.ToString());
    }
    
    private GeneratableEndpoint? TransformApiDescription(ApiDescription apiDescription)
    {
        var returnType = apiDescription.SupportedResponseTypes.FirstOrDefault(x => x.StatusCode is >= 200 and < 300 );
        
        if (returnType?.Type is null)
        {
            _logger.LogError("API endpoint with route url {RouteUrl} does not have a return type specified", apiDescription.RelativePath);
            return null;
        }
        
        var handlerName = returnType.Type?.FullName?.Split('.').Last().Split('+').FirstOrDefault();
        if (handlerName is null)
        {
            _logger.LogError("API endpoint with route url {RouteUrl} does not have a valid handler name", apiDescription.RelativePath);
            return null;
        }

        var tsTypePrefix = $"{handlerName}Request";
        GeneratableObject generatableObject = new()
        {
            Name = tsTypePrefix
        };

        var queryParams = apiDescription.ParameterDescriptions.Where(x => x.BindingInfo?.BindingSource == BindingSource.Query).ToList();
        var pathParams = apiDescription.ParameterDescriptions.Where(x => x.BindingInfo?.BindingSource == BindingSource.Path).ToList();
        var bodyParams = apiDescription.ParameterDescriptions.Where(x => x.BindingInfo?.BindingSource == BindingSource.Body).ToList();
        

        if (queryParams.Count > 0)
        {
            var generatableMember = new GeneratableMember()
            {
                Name = "Query",
            };
            
            generatableMember.Members.AddRange(GetGeneratableMembersFromApiParameterDescriptions(queryParams));
            generatableObject.Members.Add(generatableMember);
        }

        if (pathParams.Count > 0)
        {
            var generatableMember = new GeneratableMember()
            {
                Name = "Path",
            };
            
            generatableMember.Members.AddRange(GetGeneratableMembersFromApiParameterDescriptions(pathParams));
            generatableObject.Members.Add(generatableMember);
        }

        if (bodyParams.Count > 0)
        {
            var generatableMember = new GeneratableMember()
            {
                Name = "Body",
            };
            
            generatableMember.Members.AddRange(GetGeneratableMembersFromApiParameterDescriptions(bodyParams));
            generatableObject.Members.Add(generatableMember);
        }
        
        // Add the response type to the ts generation
        var responseGeneratableObject = new GeneratableObject()
        {
            Name = tsTypePrefix + "Response"
        };
        
        responseGeneratableObject.Members.AddRange(returnType.Type!.GetGeneratableMembersFromType("response", true));

        var options = new JsonSerializerOptions();
        options.Converters.Add(new CustomJsonConverterForType());
        options.WriteIndented = true;
        
        _logger.LogDebug("Request: \n{Object} \nResponse: \n{Response}", JsonSerializer.Serialize(generatableObject, options), JsonSerializer.Serialize(responseGeneratableObject, options));
        
        GeneratableEndpoint generatableEndpoint = new()
        {
            HandlerName = handlerName,
            HttpMethod = apiDescription.HttpMethod!,
            RouteUrl = apiDescription.RelativePath!,
            Version = apiDescription.GetApiVersion()?.ToString() ?? "v1",
            Request = generatableObject,
            Response = responseGeneratableObject
        };

        return generatableEndpoint;
    }

    private List<GeneratableMember> GetGeneratableMembersFromApiParameterDescriptions(List<ApiParameterDescription> parameters)
    {
        var generatableMembers = new List<GeneratableMember>();
        
        foreach (var parameter in parameters)
        {
            var parameterName = parameter.Name;
            
            generatableMembers.AddRange(parameter.Type.GetGeneratableMembersFromType(parameterName, parameterName.ToLower() is "command" or "body", _seenTypes));
        }
        
        return generatableMembers;
    }

    private List<GeneratableObject> GetSharedTypes(List<GeneratableEndpoint> generatableEndpoints)
    {
        var returnList = new List<GeneratableObject>();
        var counterDict = new Dictionary<Type, int>();

        var generatableRequestObjects = generatableEndpoints.Select(x => x.Request).ToList();
        var generatableResponseObjects = generatableEndpoints.Select(x => x.Response).ToList();
        var generatableObjects = generatableRequestObjects.Concat(generatableResponseObjects).ToList();

        var members = generatableObjects.SelectMany(x => x.Members).ToList();

        foreach (var member in members)
        {
            CountTypesFromGeneratableMember(member);
        }
        
        var sharedTypes = counterDict.Where(x => x.Value > 1).ToDictionary();
        
        foreach (var sharedType in sharedTypes.Keys)
        {
            var generatableMember = new GeneratableObject()
            {
                Name = sharedType.Name,
                ObjectType = sharedType
            };
            
            generatableMember.Members.AddRange(sharedType.GetGeneratableMembersFromSharedType(sharedType.Name, sharedTypes.Select(x => x.Key).ToList(), true));
            
            returnList.Add(generatableMember);
        }
        
        return returnList;

        void CountTypesFromGeneratableMember(GeneratableMember member)
        {
            if (member.MemberType is not null)
            {
                var memberType = member.MemberType.IsList() ? member.MemberType.GenericTypeArguments.FirstOrDefault() ?? throw new Exception("List argument is null") : member.MemberType;

                if (!memberType.IsMappablePrimitive())
                {
                    if(counterDict.TryGetValue(memberType, out var count))
                    {
                        counterDict[memberType] = count + 1;
                    }
                    else
                    {
                        counterDict.Add(memberType, 1);
                    }
                }
            }
            
            foreach (var generatableMember in member.Members)
            {
                CountTypesFromGeneratableMember(generatableMember);
            }
        }
    }
    
    private List<SharedType> GenerateSharedTypes(List<GeneratableObject> sharedTypes)
    {
        var returnableSharedTypes = new List<SharedType>();
        
        foreach (var sharedType in sharedTypes)
        {
            ITypescriptSourceBuilder sourceBuilder = new TypescriptInterfaceSourceBuilder().Initialize(sharedType.Name);
            var sharedTypeInterfaces = InterfaceGenerator.GenerateFromGeneratableObject(sharedType, sourceBuilder, new());
            var fileImportPath = $"./{sharedType.Name.ToCamelCase()}";
            var fileName = sharedType.Name.ToCamelCase() + ".ts";
            
            
            var a = 0;
        }

        return returnableSharedTypes;
    }
    
    private string GenerateFromEndpoint(GeneratableEndpoint endpoint)
    {
        var blockBuilder = new StringBuilder();

        ITypescriptSourceBuilder requestBuilder = new TypescriptInterfaceSourceBuilder().Initialize(endpoint.Request.Name);
        ITypescriptSourceBuilder responseBuilder = new TypescriptInterfaceSourceBuilder().Initialize(endpoint.Response.Name);
        
        var requestInterfaces = InterfaceGenerator.GenerateFromGeneratableObject(endpoint.Request, requestBuilder, new());
        var responseInterfaces = InterfaceGenerator.GenerateFromGeneratableObject(endpoint.Response, responseBuilder, new());
        
        blockBuilder.AppendLine(requestInterfaces);
        blockBuilder.AppendLine(responseInterfaces);
        
        _logger.LogDebug("Request: \n{Request} \nResponse: \n{Response}", requestInterfaces, responseInterfaces);
        
        return blockBuilder.ToString();
    }
    
    private void GenerateEnum(Type enumType)
    {
        if(_generatedEnums.Contains(enumType.Name))
            return;
        
        var enumBuilder = new TypescriptEnumBuilder(enumType.Name);

        var names = Enum.GetNames(enumType).ToList();
        var values = Enum.GetValues(enumType).Cast<int>().Select(x => x.ToString()).ToList();
        foreach (var name in names)
        {
            enumBuilder.AddNumberEnumProperty(name, values[names.IndexOf(name)]);
        }
        
        _enumGeneration.AppendLine(enumBuilder.Build());
        _enumGeneration.AppendLine();
        _generatedEnums.Add(enumType.Name);
    }
    
    private void GenerateCallingFunction(List<ApiParameterDescription> queryParams, List<ApiParameterDescription> pathParams, List<ApiParameterDescription> bodyParams, ApiDescription apiDescription, string tsTypePrefix, StringBuilder blockBuilder)
    {
        var codeBuilder = new StringBuilder();
        var method = apiDescription.HttpMethod;

        codeBuilder.Append(
            $"export const {tsTypePrefix.Replace("Request", "").ToCamelCase()} = (fetch: FetchType, request: {tsTypePrefix}) => ");
        
        var bodyCallString = bodyParams.Count > 0 ? "request.body" : "{}";
        
        var functionCall = method switch
        {
            "GET" =>
                $"genericGet<{tsTypePrefix}Response>(fetch, `{GenerateUrl(apiDescription.RelativePath!, queryParams, pathParams)}`);",
            "POST" =>
                $"genericPost<{tsTypePrefix}Response>(fetch, `{GenerateUrl(apiDescription.RelativePath!, queryParams, pathParams)}`, {bodyCallString});",
            "PUT" =>
                $"genericPost<{tsTypePrefix}Body, {tsTypePrefix}Response>(fetch, `{GenerateUrl(apiDescription.RelativePath!, queryParams, pathParams)}`);",
            "DELETE" =>
                $"genericPost<{tsTypePrefix}Body, {tsTypePrefix}Response>(fetch, `{GenerateUrl(apiDescription.RelativePath!, queryParams, pathParams)}`);",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        codeBuilder.Append("\n\t" + functionCall);
        
        blockBuilder.AppendLine(codeBuilder.ToString());
        blockBuilder.AppendLine();
    }

    private string GenerateUrl(string relativePath, List<ApiParameterDescription> queryParams,
        List<ApiParameterDescription> pathParams)
    {
        var queryBuilder = new UrlQueryParamBuilder();
    
        foreach (var parameter in queryParams)
        {
            queryBuilder.Add(parameter.Name.ToCamelCase(), $"${{request.query.{parameter.Name.ToCamelCase()}}}");
        }

        foreach (var parameter in pathParams)
        {
            relativePath = relativePath.Replace($"{{{parameter.Name.ToCamelCase()}}}", $"${{request.path.{parameter.ModelMetadata.PropertyName?.ToCamelCase()}}}");
        }
        
        relativePath += queryBuilder.ToString();
        
        return relativePath;
    }
}

internal class CustomJsonConverterForType : JsonConverter<Type>
{
    public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Caution: Deserialization of type instances like this is not recommended and should be avoided
        // since it can lead to potential security issues.

        // string assemblyQualifiedName = reader.GetString();
        // return Type.GetType(assemblyQualifiedName);
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.AssemblyQualifiedName);
    }
}
