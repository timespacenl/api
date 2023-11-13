using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.NewTypescriptApiClientGenerator.Extensions;
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
    private readonly Dictionary<Type, int> _typeCounter = new();

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
                FindSharedTypes(apiDescription);
                GenerateCodeForApiDescription(apiDescription);
            }
        }

        _generation.AppendLine(Constants.ApiClientHeaders);
        _generation.AppendLine(_enumGeneration.ToString());
        _generation.AppendLine(_typeGeneration.ToString());
        
        var sharedTypes = _typeCounter.Where(x => x.Value > 1).Select(x => new {x.Key, x.Value}).ToList();
        
        File.WriteAllText(_options.TypescriptGenerator.GenerationPath + '\\' + _options.TypescriptGenerator.GenerationFileName + ".ts", _generation.ToString());
    }

    private void FindSharedTypes(ApiDescription apiDescription)
    {
        var returnType = apiDescription.SupportedResponseTypes.FirstOrDefault(x => x.StatusCode is >= 200 and < 300 );
        
        if (returnType is null)
        {
            _logger.LogError("API endpoint with route url {RouteUrl} does not have a return type specified", apiDescription.RelativePath);
            return;
        }
        
        if(_typeCounter.ContainsKey(returnType.Type))
            _typeCounter[returnType.Type] += 1;
        else
            _typeCounter.Add(returnType.Type, 1);
        
        var queryParams = apiDescription.ParameterDescriptions.Where(x => x.BindingInfo?.BindingSource == BindingSource.Query).ToList();
        var pathParams = apiDescription.ParameterDescriptions.Where(x => x.BindingInfo?.BindingSource == BindingSource.Path).ToList();
        var bodyParams = apiDescription.ParameterDescriptions.Where(x => x.BindingInfo?.BindingSource == BindingSource.Body).ToList();
        
        FindSharedTypesFromApiParameterDescriptions(queryParams);
        FindSharedTypesFromApiParameterDescriptions(pathParams);
        FindSharedTypesFromApiParameterDescriptions(bodyParams);
    }

    private void FindSharedTypesFromApiParameterDescriptions(List<ApiParameterDescription> parameters)
    {
        foreach (var parameter in parameters)
        {
            var paramType = parameter.Type.IsList()
                ? parameter.Type.GenericTypeArguments.FirstOrDefault() ?? throw new Exception("List argument is null")
                : parameter.Type;

            if (_typeCounter.ContainsKey(paramType))
                _typeCounter[paramType] += 1;
            else
                _typeCounter.Add(paramType, 1);
        }
    }
    
    private void GenerateCodeForApiDescription(ApiDescription apiDescription)
    {
        var returnType = apiDescription.SupportedResponseTypes.FirstOrDefault(x => x.StatusCode is >= 200 and < 300 );

        
        if (returnType is null)
        {
            _logger.LogError("API endpoint with route url {RouteUrl} does not have a return type specified", apiDescription.RelativePath);
            return;
        }
        
        if(_typeCounter.ContainsKey(returnType.Type))
            _typeCounter[returnType.Type] += 1;
        else
            _typeCounter.Add(returnType.Type, 1);
        
        var handlerName = returnType.Type?.FullName?.Split('.').Last().Split('+').FirstOrDefault();
        if (handlerName is null)
        {
            _logger.LogError("API endpoint with route url {RouteUrl} does not have a valid handler name", apiDescription.RelativePath);
            return;
        }

        var tsTypePrefix = $"{handlerName}Request";
        var tsType = new TypescriptTypeBuilder(tsTypePrefix);

        var queryParams = apiDescription.ParameterDescriptions.Where(x => x.BindingInfo?.BindingSource == BindingSource.Query).ToList();
        var pathParams = apiDescription.ParameterDescriptions.Where(x => x.BindingInfo?.BindingSource == BindingSource.Path).ToList();
        var bodyParams = apiDescription.ParameterDescriptions.Where(x => x.BindingInfo?.BindingSource == BindingSource.Body).ToList();

        var blockBuilder = new StringBuilder();

        if (queryParams.Count > 0)
        {
            GenerateFromApiParameterDescriptions(queryParams, tsTypePrefix + "Query", blockBuilder);
            tsType.AddProperty("query", tsTypePrefix + "Query");
        }

        if (pathParams.Count > 0)
        {
            GenerateFromApiParameterDescriptions(pathParams, tsTypePrefix + "Path", blockBuilder);
            tsType.AddProperty("path", tsTypePrefix + "Path");
        }

        if (bodyParams.Count > 0)
        {
            GenerateFromApiParameterDescriptions(bodyParams, tsTypePrefix + "Body", blockBuilder);
            tsType.AddProperty("body", tsTypePrefix + "Body");
        }
        
        blockBuilder.AppendLine(tsType.Build());
        blockBuilder.AppendLine();
        
        // Add the response type to the ts generation
        var responseTsType = new TypescriptTypeBuilder(tsTypePrefix + "Response");
        GenerateFromType(returnType.Type!, tsTypePrefix, blockBuilder, responseTsType);
        
        
        GenerateCallingFunction(queryParams, pathParams, bodyParams, apiDescription, tsTypePrefix, blockBuilder);
        
        _typeGeneration.AppendLine(blockBuilder.ToString());
    }

    private void GenerateFromApiParameterDescriptions(List<ApiParameterDescription> parameters, string typePrefix, StringBuilder blockBuilder)
    {
        var tsType = new TypescriptTypeBuilder(typePrefix);
        
        foreach (var parameter in parameters)
        {
            var paramType = parameter.Type.IsList() ? parameter.Type.GenericTypeArguments.FirstOrDefault() ?? throw new Exception("List argument is null") : parameter.Type;

            if(_typeCounter.ContainsKey(paramType))
                _typeCounter[paramType] += 1;
            else
                _typeCounter.Add(paramType, 1);
            
            if (parameter.Name.ToLower() is "body" or "command")
            {
                GenerateFromType(paramType, typePrefix, blockBuilder, tsType);
                return;
            } 
            
            if (Constants.TsTypeMapping.Keys.Contains(paramType.Name))
            {
                tsType.AddProperty(parameter.Name.ToCamelCase(), paramType.GetTsType(), paramType.IsNullable(), parameter.Type.IsList());
            }
            else
            {
                if (paramType.IsEnum)
                {
                    GenerateEnum(paramType);
                    tsType.AddProperty(parameter.Name.ToCamelCase(), paramType.Name, paramType.IsNullable(), parameter.Type.IsList());
                }
                else
                {
                    GenerateFromType(paramType, typePrefix, blockBuilder);
                    tsType.AddProperty(parameter.Name.ToCamelCase(), typePrefix + paramType.Name, paramType.IsNullable(), parameter.Type.IsList());
                }
            }
        }
        
        blockBuilder.AppendLine(tsType.Build());
        blockBuilder.AppendLine();
    }
    
    private void GenerateFromType(Type type, string typePrefix, StringBuilder blockBuilder, TypescriptTypeBuilder? appendTo = null)
    {
        var tsType = appendTo ?? new TypescriptTypeBuilder(typePrefix + type.Name);
        
        foreach (var property in type.GetProperties())
        {
            var paramType = property.PropertyType.IsList() ? property.PropertyType.GenericTypeArguments.FirstOrDefault() ?? throw new Exception("List argument is null") : property.PropertyType;
        
            if(_typeCounter.ContainsKey(paramType))
                _typeCounter[paramType] += 1;
            else
                _typeCounter.Add(paramType, 1);
            
            if (Constants.TsTypeMapping.Keys.Contains(paramType.Name))
            {
                tsType.AddProperty(property.Name.ToCamelCase(), paramType.GetTsType(), paramType.IsNullable(), property.PropertyType.IsList());
            }
            else
            {
                if (paramType.IsEnum)
                {
                    GenerateEnum(paramType);
                    tsType.AddProperty(property.Name.ToCamelCase(), paramType.Name, paramType.IsNullable(), property.PropertyType.IsList());
                }
                else
                {
                    GenerateFromType(paramType, typePrefix, blockBuilder);
                    tsType.AddProperty(property.Name.ToCamelCase(), typePrefix + paramType.Name, paramType.IsNullable(), property.PropertyType.IsList());
                }
            }
        }
        
        blockBuilder.AppendLine(tsType.Build());
        blockBuilder.AppendLine();
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