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
            {"Instant", "Dayjs"},
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
          import { genericPost, genericGet } from '../api-generics';
          import type { FetchType } from '../api-generics';
          import type { Dayjs } from 'dayjs';

          """;
}

public record ValidatorMapping(string? Mapping, bool HasParameters = false, int ParameterCount = 0);