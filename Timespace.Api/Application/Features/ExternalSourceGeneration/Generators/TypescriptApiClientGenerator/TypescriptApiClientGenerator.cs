using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;
using Timespace.Api.Infrastructure.Configuration;
using Timespace.Api.Infrastructure.ExternalSourceGeneration;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator;

public class TypescriptApiClientGenerator : IExternalSourceGenerator
{
    private readonly ILogger<TypescriptApiClientGenerator> _logger;
    private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;
    private readonly StringBuilder _generation = new();
    private readonly StringBuilder _typeGeneration = new();
    private readonly StringBuilder _enumGeneration = new();
    private readonly List<string> _generatedEnums = new();
    private readonly ExternalSourceGenerationSettings _options;
    private readonly List<GeneratableEndpoint> _generatableEndpoints = new();
    private readonly HashSet<Type> _seenTypes = new();

    public TypescriptApiClientGenerator(Compilation compilation, IApiDescriptionGroupCollectionProvider apiExplorer, ILogger<TypescriptApiClientGenerator> logger, IOptions<ExternalSourceGenerationSettings> options)
    {
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
            }
        }
        
        Directory.Delete(_options.TypescriptGenerator.GenerationRoot + "/routes", true);
        Directory.Delete(_options.TypescriptGenerator.GenerationRoot + "/enums", true);
        Directory.Delete(_options.TypescriptGenerator.GenerationRoot + "/shared", true);
        var sharedTypeObjects = GetSharedTypes(_generatableEndpoints);
        var sharedTypes = GenerateSharedTypes(sharedTypeObjects);
        
        
        foreach (var endpoint in _generatableEndpoints)
        {
            GenerateFromEndpoint(endpoint, sharedTypes);
        }
    }
    
    private GeneratableEndpoint? TransformApiDescription(ApiDescription apiDescription)
    {
        var returnType = apiDescription.SupportedResponseTypes.FirstOrDefault(x => x.StatusCode is >= 200 and < 300 );
        
        if (returnType?.Type is null)
        {
            _logger.LogError("API endpoint with route url {RouteUrl} does not have a return type specified", apiDescription.RelativePath);
            return null;
        }

        string? handlerName = null;
        if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            handlerName = controllerActionDescriptor.ActionName;
        
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
        var formParams = apiDescription.ParameterDescriptions.Where(x => x.Source == BindingSource.Form).ToList();

        if(bodyParams.Any() && formParams.Any())
        {
            _logger.LogError("API endpoint with route url {RouteUrl} has both body and form parameters", apiDescription.RelativePath);
            return null;
        }
        
        if (queryParams.Count > 0)
        {
            Type? memberType = null;
            if(queryParams.Count == 1 && !queryParams.FirstOrDefault()!.Type.IsMappablePrimitive())
            {
                memberType = queryParams.FirstOrDefault()?.Type;
            }
            
            var generatableMember = new GeneratableMember()
            {
                Name = "Query",
                MemberType = memberType
            };
            
            generatableMember.Members.AddRange(GetGeneratableMembersFromApiParameterDescriptions(queryParams));
            generatableObject.Members.Add(generatableMember);
        }

        if (pathParams.Count > 0)
        {
            Type? memberType = null;
            if(pathParams.Count == 1 && !pathParams.FirstOrDefault()!.Type.IsMappablePrimitive())
            {
                memberType = pathParams.FirstOrDefault()?.Type;
            }
            var generatableMember = new GeneratableMember()
            {
                Name = "Path",
                MemberType = memberType
            };
            
            generatableMember.Members.AddRange(GetGeneratableMembersFromApiParameterDescriptions(pathParams));
            generatableObject.Members.Add(generatableMember);
        }

        if (bodyParams.Count > 0)
        {
            Type? memberType = null;
            if(bodyParams.Count == 1 && !bodyParams.FirstOrDefault()!.Type.IsMappablePrimitive())
            {
                memberType = bodyParams.FirstOrDefault()?.Type;
            }
            
            var generatableMember = new GeneratableMember()
            {
                Name = "Body",
                MemberType = memberType
            };
            
            generatableMember.Members.AddRange(GetGeneratableMembersFromApiParameterDescriptions(bodyParams));
            generatableObject.Members.Add(generatableMember);
        }
        
        if (formParams.Count > 0)
        {
            Type? memberType = null;
            var rootValues = formParams.Where(x => x.Name.Count(c => c == '.') == 1 || !x.Name.Contains('.')).ToList();
            var rootItem = rootValues.FirstOrDefault();

            if (rootItem is not null)
            {
                var rootType = rootItem.ModelMetadata.ContainerType;
                memberType = rootType;
            }
            
            if(rootValues.Count == 1 && !rootValues.FirstOrDefault()!.Type.IsMappablePrimitive())
            {
                memberType = bodyParams.FirstOrDefault()?.Type;
            }
            
            var generatableMember = new GeneratableMember()
            {
                Name = "Body",
                MemberType = memberType
            };

            if (memberType is not null)
                generatableMember.Members.AddRange(memberType.GetGeneratableMembersFromType("body", true, _seenTypes));
            else
                generatableMember.Members.AddRange(GetGeneratableMembersFromApiParameterDescriptions(rootValues, true));
            
            generatableObject.Members.Add(generatableMember);
        }
        
        // Add the response type to the ts generation
        var responseGeneratableObject = new GeneratableObject()
        {
            Name = handlerName + "Response",
            ObjectType = returnType.Type
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
            Response = responseGeneratableObject,
            OriginalApiDescription = apiDescription
        };

        return generatableEndpoint;
    }

    private List<GeneratableMember> GetGeneratableMembersFromApiParameterDescriptions(List<ApiParameterDescription> parameters, bool formData = false)
    {
        var generatableMembers = new List<GeneratableMember>();
        
        foreach (var parameter in parameters)
        {
            var parameterName = formData ? parameter.Name.Split('.').Last() : parameter.Name;
            
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

        foreach (var generatableObject in generatableObjects)
        {
            if(generatableObject.ObjectType is null)
                continue;
            
            var memberType = generatableObject.ObjectType.IsEnumerableT() ? generatableObject.ObjectType.GenericTypeArguments.FirstOrDefault() ?? throw new Exception("List argument is null") : generatableObject.ObjectType;

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
        
        var members = generatableObjects.SelectMany(x => x.Members).ToList();

        foreach (var member in members)
        {
            CountTypesFromGeneratableMember(member);
        }
        
        var sharedTypes = counterDict.Where(x => x.Value > 1 || x.Key.IsEnum).ToDictionary();
        
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
                var memberType = member.MemberType;

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
    
    private List<SharedType> GenerateSharedTypes(List<GeneratableObject> sharedTypesObjects)
    {
        var returnableSharedTypes = new List<SharedType>();
        var localSharedTypes = new List<SharedType>();

        if(!Directory.Exists(_options.TypescriptGenerator.GenerationRoot + "/shared"))
            Directory.CreateDirectory(_options.TypescriptGenerator.GenerationRoot + "/shared");
        
        if(!Directory.Exists(_options.TypescriptGenerator.GenerationRoot + "/enums"))
            Directory.CreateDirectory(_options.TypescriptGenerator.GenerationRoot + "/enums");
        
        foreach (var sharedType in sharedTypesObjects)
        {
            var typeName = sharedType.Name;
            if(sharedType.ObjectType is not null && sharedType.ObjectType.IsGenericType)
            {
                typeName = (sharedType.Name.Split('`').FirstOrDefault() ?? "") + "Of" + sharedType.ObjectType.GenericTypeArguments.FirstOrDefault()?.Name;
            }
            
            var fileImportPath = $"@api-client/shared/{typeName.ToCamelCase()}";
            var fileImport = $"import {{ to{typeName}, from{typeName}, type {typeName} }} from '{fileImportPath}';";
            if(sharedType.ObjectType is not null && sharedType.ObjectType.IsEnum)
            {
                fileImport = $"import {{ {typeName} }} from '{fileImportPath}';";
            }
            
            var sharedTypeObject = new SharedType(sharedType.ObjectType, sharedType.Name, $"to{typeName}", $"from{typeName}", fileImportPath, fileImport);
            localSharedTypes.Add(sharedTypeObject);
        }
        
        foreach (var sharedType in sharedTypesObjects)
        {
            var typeName = sharedType.Name;
            if(sharedType.ObjectType is not null && sharedType.ObjectType.IsGenericType)
            {
                typeName = (sharedType.Name.Split('`').FirstOrDefault() ?? "") + "Of" + sharedType.ObjectType.GenericTypeArguments.FirstOrDefault()?.Name ?? "";
            }

            if (sharedType.ObjectType is not null && sharedType.ObjectType.IsEnum)
            {
                var enumGeneration = GenerateEnum(sharedType.ObjectType);
                File.WriteAllText(_options.TypescriptGenerator.GenerationRoot + $"/enums/{typeName.ToCamelCase()}.ts", enumGeneration);
            }
            else
            {
                var fileContentBuilder = new StringBuilder();
                ITypescriptSourceBuilder interfaceSourceBuilder = new TypescriptInterfaceSourceBuilder().Initialize(typeName, true);
                ITypescriptSourceBuilder toMappingSourceBuilder = new TypescriptToMappingBuilder().Initialize(typeName, true);
                ITypescriptSourceBuilder fromMappingSourceBuilder = new TypescriptFromMappingBuilder().Initialize(typeName, true);
                var importables = new HashSet<TypescriptImportable>();
                var localSharedTypesWithoutCurrent = localSharedTypes.Where(x => x.OriginalType != sharedType.ObjectType).ToList();
                var sharedTypeInterfaces = ApiClientSourceBuilder<TypescriptInterfaceSourceBuilder>.GenerateFromGeneratableObject(sharedType, interfaceSourceBuilder, localSharedTypesWithoutCurrent, importables);
                var sharedTypeToMapping = ApiClientSourceBuilder<TypescriptToMappingBuilder>.GenerateFromGeneratableObject(sharedType, toMappingSourceBuilder, localSharedTypesWithoutCurrent, importables);
                var sharedTypeFromMapping = ApiClientSourceBuilder<TypescriptFromMappingBuilder>.GenerateFromGeneratableObject(sharedType, fromMappingSourceBuilder, localSharedTypesWithoutCurrent, importables);
                var importString = GetImportsFromTypeList(importables, localSharedTypes);
                
                fileContentBuilder.Append(importString);
                fileContentBuilder.Append(sharedTypeInterfaces);
                fileContentBuilder.Append(sharedTypeToMapping);
                fileContentBuilder.Append(sharedTypeFromMapping);
                
                File.WriteAllText(_options.TypescriptGenerator.GenerationRoot + $"/shared/{typeName.ToCamelCase()}.ts", fileContentBuilder.ToString());
            }
            
            var fileImportPath = $"@api-client/shared/{typeName.ToCamelCase()}";
            var fileImport = $"import {{ to{typeName}, from{typeName}, type {typeName} }} from '{fileImportPath}';";
            if(sharedType.ObjectType is not null && sharedType.ObjectType.IsEnum)
            {
                fileImportPath = $"@api-client/enums/{typeName.ToCamelCase()}";
                fileImport = $"import {{ {typeName} }} from '{fileImportPath}';";
            }
            
            var sharedTypeObject = new SharedType(sharedType.ObjectType, sharedType.Name, $"to{typeName}", $"from{typeName}", fileImportPath, fileImport);
            returnableSharedTypes.Add(sharedTypeObject);
        }

        return returnableSharedTypes;
    }
    
    private string GenerateFromEndpoint(GeneratableEndpoint endpoint, List<SharedType> sharedTypes)
    {
        var dirPath = _options.TypescriptGenerator.GenerationRoot + $"/routes/{endpoint.RouteUrl.Replace('{', '(').Replace('}', ')').Replace(':', '_')}";
        Directory.CreateDirectory(dirPath);
        
        var blockBuilder = new StringBuilder();

        ITypescriptSourceBuilder requestInterfacesBuilder = new TypescriptInterfaceSourceBuilder().Initialize(endpoint.Request.Name);
        ITypescriptSourceBuilder responseInterfacesBuilder = new TypescriptInterfaceSourceBuilder().Initialize(endpoint.Response.Name);
        ITypescriptSourceBuilder requestToMappingBuilder = new TypescriptToMappingBuilder().Initialize(endpoint.Request.Name);
        ITypescriptSourceBuilder responseToMappingBuilder = new TypescriptToMappingBuilder().Initialize(endpoint.Response.Name);
        ITypescriptSourceBuilder requestFromMappingBuilder = new TypescriptFromMappingBuilder().Initialize(endpoint.Request.Name);
        ITypescriptSourceBuilder responseFromMappingBuilder = new TypescriptFromMappingBuilder().Initialize(endpoint.Response.Name);
        
        var importables = new HashSet<TypescriptImportable>();
        var requestInterfaces = ApiClientSourceBuilder<TypescriptInterfaceSourceBuilder>.GenerateFromGeneratableObject(endpoint.Request, requestInterfacesBuilder, sharedTypes, importables);
        var responseInterfaces = ApiClientSourceBuilder<TypescriptInterfaceSourceBuilder>.GenerateFromGeneratableObject(endpoint.Response, responseInterfacesBuilder, sharedTypes, importables);
        var requestToMapping = ApiClientSourceBuilder<TypescriptToMappingBuilder>.GenerateFromGeneratableObject(endpoint.Request, requestToMappingBuilder, sharedTypes, importables);
        var responseToMapping = ApiClientSourceBuilder<TypescriptToMappingBuilder>.GenerateFromGeneratableObject(endpoint.Response, responseToMappingBuilder, sharedTypes, importables);
        var requestFromMapping = ApiClientSourceBuilder<TypescriptFromMappingBuilder>.GenerateFromGeneratableObject(endpoint.Request, requestFromMappingBuilder, sharedTypes, importables);
        var responseFromMapping = ApiClientSourceBuilder<TypescriptFromMappingBuilder>.GenerateFromGeneratableObject(endpoint.Response, responseFromMappingBuilder, sharedTypes, importables);
        
        var imports = GetImportsFromTypeList(importables, sharedTypes);

        if(importables.Count > 0) blockBuilder.Append(imports);
        blockBuilder.Append(requestInterfaces);
        blockBuilder.Append(responseInterfaces);
        // blockBuilder.Append(requestToMapping);
        blockBuilder.Append(responseToMapping);
        blockBuilder.Append(requestFromMapping);
        // blockBuilder.Append(responseFromMapping);
        
        File.WriteAllText(dirPath + $"/{endpoint.HandlerName.ToCamelCase()}.ts", blockBuilder.ToString());
        
        return blockBuilder.ToString();
    }
    
    private string GenerateEnum(Type enumType)
    {
        var enumBuilder = new TypescriptEnumBuilder(enumType.Name);

        var names = Enum.GetNames(enumType).ToList();
        var values = Enum.GetValues(enumType).Cast<int>().Select(x => x.ToString()).ToList();
        foreach (var name in names)
        {
            enumBuilder.AddNumberEnumProperty(name, values[names.IndexOf(name)]);
        }

        return enumBuilder.Build();
    }

    private string GetImportsFromTypeList(HashSet<TypescriptImportable> types, List<SharedType> sharedTypes)
    {
        var importBuilder = new StringBuilder();
        foreach (var type in types)
        {
            if (type.ImportType == ImportType.TYPE)
            {
                var sharedType = sharedTypes.FirstOrDefault(x => x.OriginalType == type.ImportableType);
                
                if(sharedType is null) continue;
                
                importBuilder.AppendLine(sharedType.ImportString);
            } else if (type.ImportType == ImportType.DAYJS)
            {
                importBuilder.AppendLine("import dayjs, { type Dayjs } from 'dayjs';");
            }
        }

        return importBuilder.ToString();
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
