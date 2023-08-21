namespace Timespace.Api.Application.Features.ExternalSourceGeneration;

public static class Constants
{
    public static readonly Dictionary<string, string> TsTypeMapping = new Dictionary<string, string>()
        {
            {"String", "string"},
            {"Guid", "string"},
            {"Int32", "number"},
            {"Double", "number"},
            {"Boolean", "boolean"},
            {"Instant", "Date"},
        };

    public static readonly Dictionary<string, string> ZodTypeMapping = new Dictionary<string, string>()
        {
            {"String", "string"},
            {"Guid", "string"},
            {"Int32", "number"},
            {"Double", "number"},
            {"Boolean", "boolean"},
            {"Instant", "date"},
        };

    public static readonly Dictionary<string, ValidatorMapping> ZodFluentValidationValidatorMapping = new Dictionary<string, ValidatorMapping>()
        {
            {"NotEmpty", new ValidatorMapping(null)},
            {"NotNull", new ValidatorMapping(null)},
            {"WithMessage", new ValidatorMapping(null)},
            {"OneOf", new ValidatorMapping(null)},
            {"Equal", new ValidatorMapping(null)},
            {"MaximumLength", new ValidatorMapping("max", true, 1)},
            {"MinimumLength", new ValidatorMapping("min", true, 1)},
            {"GreaterThanOrEqualTo", new ValidatorMapping("gte", true, 1)},
            {"LessThanOrEqualTo", new ValidatorMapping("lte", true, 1)},
            {"EmailAddress", new ValidatorMapping("email")},
        };

    public static readonly string ApiClientHeaders = """
import { PUBLIC_BASE_URL } from '$env/static/public';

type ApiResponse<T> = {
  data: T | null;
  error: ProblemDetails | null;
}

export type FetchType = typeof fetch;

export interface ProblemDetails {
    type: string;
    title: string;
    status: number;
    detail: string | null;
    instance: string;
}


""";
}

public record ValidatorMapping(string? Mapping, bool HasParameters = false, int ParameterCount = 0);