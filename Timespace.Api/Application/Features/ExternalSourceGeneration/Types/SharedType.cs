namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

public record SharedType(
    Type? OriginalType,
    string InterfaceName,
    string ToMappingFunction,
    string FromMappingFunction,
    string ImportPath,
    string ImportString
    );
