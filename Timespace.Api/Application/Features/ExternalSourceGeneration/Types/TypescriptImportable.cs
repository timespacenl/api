using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

public record TypescriptImportable(ImportType ImportType, Type? ImportableType);
