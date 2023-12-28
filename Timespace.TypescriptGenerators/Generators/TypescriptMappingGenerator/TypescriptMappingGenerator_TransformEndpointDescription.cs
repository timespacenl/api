using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using TimeSpace.Shared.TypescriptGenerator;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Extensions;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator;

public partial class TypescriptMappingGenerator
{
    public ApiEndpoint? TransformEndpointDescription(EndpointDescription description)
    {
        var allNodes = _compilation.SyntaxTrees.SelectMany(x => x.GetRoot().DescendantNodes()).ToList();

        var declaringType = _compilation.GetTypeByMetadataName(description.ControllerTypeName);

        if (declaringType is null)
        {
            _logger.LogError("API endpoint with route url {RouteUrl} does not have a parent controller type that could be found", description.RelativePath);
            return null;
        }
        
        var actionSym = declaringType.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(x => x.Name == description.ActionName);

        if(actionSym is null)
        {
            _logger.LogError("API endpoint with route url {RouteUrl} does not have a valid action symbol that could be found.", description.RelativePath);
            return null;
        }

        var actionName = actionSym.Name.Replace("Async", "");
        
        var returnType = actionSym.ReturnType;
        
        var endpointParameters = TransformParameters(actionSym, description.RelativePath);
        
        Dictionary<string, ApiType> requestTypes = new();
        
        var bodyParams = endpointParameters.Where(x => x.Source is ParameterSource.Body or ParameterSource.Form).ToList();
        var pathParams = endpointParameters.Where(x => x.Source == ParameterSource.Path).ToList();
        var queryParams = endpointParameters.Where(x => x.Source == ParameterSource.Query).ToList();
        
        string? bodyTypeName = null;
        if (bodyParams.Count > 0)
        {
            if (bodyParams.Count == 1)
                bodyTypeName = bodyParams.First().TypeSymbol.ToFullyQualifiedDisplayString();
            else
                bodyTypeName = actionName + "RequestBody";
            
            ProcessEndpointParameters(bodyParams, "RequestBody", requestTypes);
        }
        
        string? pathTypeName = null;
        if (pathParams.Count > 0)
        {
            var firstParamTypeDefinedBy = pathParams.First().DefinedBy;
            if (pathParams.DistinctBy(x => x.DefinedBy).Count() == 1 && firstParamTypeDefinedBy is not null)
                pathTypeName = firstParamTypeDefinedBy.ToFullyQualifiedDisplayString();
            else
                pathTypeName = actionName + "RequestPath";
            
            ProcessEndpointParameters(pathParams, "RequestPath", requestTypes);
        }

        string? queryTypeName = null;
        if (queryParams.Count > 0)
        {
            var firstParamTypeDefinedBy = queryParams.First().DefinedBy;
            if (queryParams.DistinctBy(x => x.DefinedBy?.Name).Count() == 1 && firstParamTypeDefinedBy is not null)
                queryTypeName = firstParamTypeDefinedBy.ToFullyQualifiedDisplayString();
            else
                queryTypeName = actionName + "RequestQuery";
                
            
            ProcessEndpointParameters(queryParams, "RequestQuery", requestTypes);
        }
        
        Dictionary<string, ApiType> responseTypes = new();
        var responseTypeName = UnwrapType(returnType).ToFullyQualifiedDisplayString();
        
        GenerateApiTypeFromTypeSymbol(returnType, actionName, responseTypes);

        var endpoint = new ApiEndpoint()
        {
            RequestTypes = requestTypes,
            ResponseTypes = responseTypes,
            BodyTypeName = bodyTypeName,
            QueryTypeName = queryTypeName,
            PathTypeName = pathTypeName,
            ResponseTypeName = responseTypeName,
            HttpMethod = description.HttpMethod!,
            RouteUrl = description.RelativePath,
            Version = description.Version ?? "v1"
        };

        return endpoint;
        
        // var props = new List<ApiClassProperty>();
        // props.AddRange(_generatableTypes.Values.OfType<ApiTypeWrapper>().SelectMany(x => x.Properties));
        // props.AddRange(_generatableTypes.Values.OfType<ApiTypeClass>().SelectMany(x => x.Properties));
        // var sharedTypeNames = props.DistinctBy(x => x.FullyQualifiedTypeName.Replace("?", "")).Where(x => !TypeSymbolExtensions.IsDefaultMappable(x.FullyQualifiedTypeName)).Select(x => new
        // {
        //     Name = x.FullyQualifiedTypeName,
        //     Count = props.Count(y => y.FullyQualifiedTypeName == x.FullyQualifiedTypeName)
        // }).Where(x => x.Count > 1).Select(x => x.Name).ToList();
        //
        // var sharedTypes = _generatableTypes.Where(x => sharedTypeNames.Contains(x.Key)).ToList();

        void ProcessEndpointParameters(List<EndpointParameter> parameters, string typeNameSuffix, Dictionary<string, ApiType> generatableTypes)
        {
            if (parameters.Count == 1 && !parameters[0].TypeSymbol.IsDefaultMappable())
            {
                var typeSymbol = UnwrapType(parameters[0].TypeSymbol);

                GenerateApiTypeFromTypeSymbol(typeSymbol, actionName, generatableTypes);
            }
            else
            {
                var firstParamTypeDefinedBy = parameters.First().DefinedBy;
                if (parameters.DistinctBy(x => x.DefinedBy).Count() > 1 && firstParamTypeDefinedBy is not null)
                    GenerateApiTypeFromTypeSymbol(firstParamTypeDefinedBy, actionName, generatableTypes);
                else
                    GenerateApiTypeFromEndpointParameters(parameters, actionName, typeNameSuffix, generatableTypes);
            }
        }
    }

    private void GenerateApiTypeFromEndpointParameters(List<EndpointParameter> endpointParameters, string typeNamePrefix, string typeName, Dictionary<string, ApiType> generatableTypes)
    {
        var source = endpointParameters.First().Source;
        if(endpointParameters.Any(x => x.Source != source))
            throw new ArgumentException("All endpoint parameters must have the same source type.");

        var properties = new List<ApiClassProperty>();
        
        foreach (var endpointParameter in endpointParameters)
        {
            var paramType = endpointParameter.TypeSymbol;
            var paramName = paramType.GetNameFromBindingAttributeIfExists() ?? endpointParameter.Name;
            if (paramType.IsDefaultMappable())
            {
                properties.Add(GetClassPropertyFromTypeSymbol(paramType, paramName, typeNamePrefix));
            }
            else
            {
                var typeMembers = paramType.GetMembers().OfType<IPropertySymbol>().ToList();
                foreach (var typeMember in typeMembers)
                {
                    if(typeMember.Name == "EqualityContract")
                        continue;
                    
                    properties.Add(GetClassPropertyFromPropertySymbol(typeMember, typeNamePrefix));
                }
            }
        }
        
        var apiType = new ApiTypeWrapper()
        {
            TypeName = typeNamePrefix + typeName,
            Properties = properties
        };
        
        generatableTypes.Add(typeNamePrefix + typeName, apiType);
    }
    
    private CollectionInfo GetCollectionInfoFromTypeSymbol(ITypeSymbol typeSymbol)
    {
        if(typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            return CollectionInfoHelpers.None;
        
        if (namedTypeSymbol.IsCollectionType())
        {
            if (namedTypeSymbol.IsDictionaryType())
            {
                if (namedTypeSymbol.TypeArguments[0].IsDefaultMappable() && !namedTypeSymbol.TypeArguments[1].IsCollectionType())
                    return CollectionInfoHelpers.Dictionary(namedTypeSymbol.TypeArguments[0].Name, namedTypeSymbol.TypeArguments[0].ToFullyQualifiedDisplayString(), namedTypeSymbol.TypeArguments[1].Name,
                        namedTypeSymbol.TypeArguments[1].ToFullyQualifiedDisplayString());
            }
            else
            {
                if(!namedTypeSymbol.TypeArguments[0].IsCollectionType())
                    return CollectionInfoHelpers.List(namedTypeSymbol.TypeArguments[0].Name, namedTypeSymbol.TypeArguments[0].ToFullyQualifiedDisplayString());
            }
            
            throw new Exception("Nested collections are not supported.");
        }
        
        return CollectionInfoHelpers.None;
    }

    private void GenerateApiTypeFromTypeSymbol(ITypeSymbol typeSymbol, string typeNamePrefix, Dictionary<string, ApiType> generatableTypes)
    {
        typeSymbol = UnwrapType(typeSymbol);
        var properties = new List<ApiClassProperty>();
        var propertySymbols = typeSymbol.GetMembers().OfType<IPropertySymbol>().ToList();
        
        foreach (var propertySymbol in propertySymbols)
        {
            if(propertySymbol.Name == "EqualityContract")
                continue;
            
            properties.Add(GetClassPropertyFromPropertySymbol(propertySymbol, typeNamePrefix));
            var resolvedPropertySymbolType = UnwrapType(propertySymbol.Type);
            if (!propertySymbol.Type.IsDefaultMappable() && 
                !resolvedPropertySymbolType.IsDefaultMappable() && 
                resolvedPropertySymbolType.Name != typeSymbol.Name && 
                !generatableTypes.ContainsKey(resolvedPropertySymbolType.ToFullyQualifiedDisplayString().Replace("?", "")))
            {
                GenerateApiTypeFromTypeSymbol(propertySymbol.Type, typeNamePrefix, generatableTypes);
            }
        }
        
        var apiType = new ApiTypeClass()
        {
            TypeName = typeSymbol.GetNameFromBindingAttributeIfExists() ?? GetTypeSymbolName(typeSymbol),
            TypePrefix = typeNamePrefix,
            FullyQualifiedTypeName = typeSymbol.ToFullyQualifiedDisplayString(),
            Properties = properties
        };
        
        _ = generatableTypes.TryAdd(typeSymbol.ToFullyQualifiedDisplayString(), apiType);
    }
    
    private string GetTypeSymbolName(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol {IsGenericType: true } namedTypeSymbol)
        {
            if (!namedTypeSymbol.IsCollectionType())
            {
                var firstTypeArg = namedTypeSymbol.TypeArguments[0];

                var typeName = firstTypeArg.Name;
                
                if(namedTypeSymbol.IsWrappedNullable())
                    return "Nullable" + typeName;
                if(namedTypeSymbol.IsPassthroughType())
                    return typeName;
                
                return namedTypeSymbol.Name + "Of" + string.Join("And", namedTypeSymbol.TypeArguments.Select(GetTypeSymbolName));
            }
        }
        
        return typeSymbol.Name;
    }

    private ApiClassProperty GetClassPropertyFromPropertySymbol(IPropertySymbol propertySymbol, string typeNamePrefix)
    {
        var propertyName = propertySymbol.GetNameFromBindingAttributeIfExists() ?? propertySymbol.Name;
        var propertyType = UnwrapType(propertySymbol.Type);
        var propertyFqtn = propertyType.ToFullyQualifiedDisplayString();
        
        return new()
        {
            CollectionInfo = GetCollectionInfoFromTypeSymbol(propertySymbol.Type),
            Name = propertyName,
            IsNullable = propertyType.IsNullable(),
            TypeName = GetTypeSymbolName(propertyType),
            TypePrefix = typeNamePrefix,
            FullyQualifiedTypeName = propertyFqtn
        };
    }
    
    private ApiClassProperty GetClassPropertyFromTypeSymbol(ITypeSymbol typeSymbol, string propertyName, string typeNamePrefix)
    {
        var propertyType = UnwrapType(typeSymbol);
        var propertyFqtn = propertyType.ToFullyQualifiedDisplayString();
        
        return new()
        {
            CollectionInfo = GetCollectionInfoFromTypeSymbol(typeSymbol),
            Name = propertyName,
            IsNullable = propertyType.IsNullable(),
            TypeName = GetTypeSymbolName(propertyType),
            TypePrefix = typeNamePrefix,
            FullyQualifiedTypeName = propertyFqtn
        };
    }

    private ITypeSymbol UnwrapType(ITypeSymbol typeSymbol)
    {
        ITypeSymbol returnable = typeSymbol;
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.IsCollectionType())
            {
                if (namedTypeSymbol.IsDictionaryType())
                {
                    returnable = namedTypeSymbol.TypeArguments[1];
                }
                else
                {
                    returnable = namedTypeSymbol.TypeArguments[0];
                }

            } else if (namedTypeSymbol.IsPassthroughType())
            {
                returnable = namedTypeSymbol.TypeArguments[0];
            }
        }

        return returnable;
    }
}
