using Microsoft.CodeAnalysis;
using TimeSpace.Shared.TypescriptGenerator;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

public record EndpointParameter(string Name, ITypeSymbol Type, ParameterSource Source);
