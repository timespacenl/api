﻿using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using TimeSpace.Shared.TypescriptGenerator;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Extensions;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator;

public partial class TypescriptMappingGenerator
{
    private List<EndpointParameter> TransformParameters(IMethodSymbol actionSymbol, string routeUrl)
    {
        var transformedParameters = new List<EndpointParameter>();

        if(actionSymbol.Parameters.Length != 1)
            throw new ArgumentException("Action must have exactly one parameter.");
        
        var actionParameter = actionSymbol.Parameters.First();
        
        var source = actionParameter.GetSymbolBindingSource() ?? (actionParameter.Type.IsDefaultMappable() ? ParameterSource.Query : ParameterSource.Body);
        
        if(source != ParameterSource.Query)
            transformedParameters.Add(new EndpointParameter(
                actionParameter.GetNameFromBindingAttributeIfExists() ?? actionParameter.Name,
                actionParameter.Type,
                null,
                source)
            );
        else 
            GetParametersFromTypeSymbol(actionParameter.Type, actionParameter.Type, transformedParameters);
        
        return transformedParameters;
    }
    
    private void GetParametersFromTypeSymbol(ITypeSymbol root, ITypeSymbol typeSymbol, List<EndpointParameter> transformedParameters)
    {
        var typeMembers = typeSymbol.GetMembers().OfType<IPropertySymbol>().ToList();
        foreach (var typeMember in typeMembers)
        {
            if(typeMember.Name == "EqualityContract")
                continue;
            
            var source = typeMember.GetSymbolBindingSource() ?? ParameterSource.Query;
            
            if(typeMember.Type.IsDefaultMappable() || source is ParameterSource.Body or ParameterSource.Form)
                transformedParameters.Add(new EndpointParameter(
                    typeMember.GetNameFromBindingAttributeIfExists() ?? typeMember.Name,
                    typeMember.Type,
                     root.Name != typeMember.ContainingType.Name ? typeMember.ContainingType : null,
                    source));
            else
                GetParametersFromTypeSymbol(root, typeMember.Type, transformedParameters);
        }
    }
}