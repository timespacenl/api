namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator;

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
            {"LocalDate", "Dayjs"},
            {"IFormFile", "File"}
        };

    public static readonly List<string> BuiltInTypes = [
        "int",
        "string",
        "bool",
        "byte",
        "sbyte",
        "char",
        "decimal",
        "double",
        "float",
        "int",
        "uint",
        "nint",
        "nuint",
        "long",
        "ulong",
        "short",
        "ushort"
    ];

    public static readonly string ApiClientHeaders = 
         """
         import { genericPost, genericGet } from '../api-generics';
         import type { FetchType } from '../api-generics';
         import type { Dayjs } from 'dayjs';

         """;
}
