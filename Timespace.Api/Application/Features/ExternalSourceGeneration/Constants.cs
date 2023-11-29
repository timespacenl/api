namespace Timespace.Api.Application.Features.ExternalSourceGeneration;

public static class Constants
{
    public static readonly Dictionary<string, string> MappableTypesMapping = new Dictionary<string, string>()
        {
            {"String", "string"},
            {"Guid", "string"},
            {"Int32", "number"},
            {"Double", "number"},
            {"Boolean", "boolean"},
            {"Instant", "Dayjs"},
            {"IFormFile", "File"}
        };

    public static readonly string ApiClientHeaders = """
          import { genericPost, genericGet } from '../api-generics';
          import type { FetchType } from '../api-generics';
          import type { Dayjs } from 'dayjs';

          """;
}
