using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using TimeSpace.Shared.TypescriptGenerator;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Extensions;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator;

public partial class TypescriptMappingGenerator
{
    public GeneratableEndpoint? TransformEndpointDescription(EndpointDescription description)
    {
        var allNodes = _compilation.SyntaxTrees.SelectMany(x => x.GetRoot().DescendantNodes()).ToList();

        var declaringType = _compilation.GetTypeByMetadataName(description.ControllerTypeName);

        if (declaringType is null)
        {
            _logger.LogError("API endpoint with route url {RouteUrl} does not have a parent controller type that could be found", description.RelativePath);
            return null;
        }
        
        var actionSym = declaringType.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(x => x.Name == description.ActionName);
        var actionParams = actionSym.Parameters.Select(x =>
        {
            return new
            {
                name = x.Name,
                attrs = x.GetAttributes()
            };
        });

        var endpointParameters = TransformParameters(description.Parameters, actionSym, description.RelativePath);

        return null;
    }
    
    private List<EndpointParameter> TransformParameters(List<ParameterDescription> endpointParameters, IMethodSymbol actionSymbol, string routeUrl)
    {
        var actionParameters = actionSymbol.Parameters;
        var transformedParameters = new List<EndpointParameter>();

        foreach (var actionParameter in actionParameters)
        {
            if(HandlePathParameter(transformedParameters, actionParameter, routeUrl))
                continue;
            
            if(HandleQueryParameter(transformedParameters, actionParameter, routeUrl))
                continue;
            
            var source = GetSource(actionParameter.GetAttributes()) ?? (actionParameter.Type.IsBuiltInType() ? ParameterSource.Query : ParameterSource.Body);

            transformedParameters.Add(new EndpointParameter(
                actionParameter.GetNameFromBindingAttributeIfExists() ?? actionParameter.Name,
                actionParameter.Type,
                source));
        }
        
        return transformedParameters;
    }
    
    private bool HandlePathParameter(List<EndpointParameter> transformedParameters, IParameterSymbol actionParameter, string routeUrl)
    {
        if (routeUrl.Contains($"{{{actionParameter.Name}}}"))
        {
            transformedParameters.Add(new EndpointParameter(
                actionParameter.GetNameFromBindingAttributeIfExists() ?? actionParameter.Name,
                actionParameter.Type,
                ParameterSource.Path));

            return true;
        }

        return false;
    }
    
    private bool HandleQueryParameter(List<EndpointParameter> transformedParameters, IParameterSymbol actionParameter, string routeUrl)
    {
        if (actionParameter.IsQuerySource() && !actionParameter.Type.IsBuiltInType())
        {
            var paramMembers = actionParameter.Type.GetMembers().OfType<IPropertySymbol>().ToList();
                
            if (paramMembers.Any(x => x.IsNotQuerySource()))
            {
                foreach (var paramMember in paramMembers)
                {
                    if(paramMember.Name == "EqualityContract")
                        continue;
                        
                    var source = GetSource(paramMember.GetAttributes()) ?? ParameterSource.Query;

                    transformedParameters.Add(new EndpointParameter(
                        paramMember.GetNameFromBindingAttributeIfExists() ?? paramMember.Name,
                        paramMember.Type,
                        source));
                }
            }
            else
            {
                var source = GetSource(actionParameter.GetAttributes()) ?? ParameterSource.Query;

                transformedParameters.Add(new EndpointParameter(
                    actionParameter.GetNameFromBindingAttributeIfExists() ?? actionParameter.Name,
                    actionParameter.Type,
                    source));
            }

            return true;
        }
        
        return false;
    }

    private ParameterSource? GetSource(IEnumerable<AttributeData> attributes)
    {
        foreach (var attributeData in attributes)
        {
            var name = attributeData.AttributeClass?.Name;
            if (name == "FromQueryAttribute")
                return ParameterSource.Query;
            if (name == "FromRouteAttribute")
                return ParameterSource.Path;
            if (name == "FromBodyAttribute")
                return ParameterSource.Body;
            if (name == "FromFormAttribute")
                return ParameterSource.Form;
        }

        return null;
    }
}
