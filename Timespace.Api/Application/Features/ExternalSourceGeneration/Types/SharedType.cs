namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

public record SharedType(
    Type SharedTypeType,
    string TypeName,
    string InterfaceName,
    string ToMappingFunction,
    string FromMappingFunction,
    string ImportPath
    );
