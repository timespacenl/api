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

    public static readonly List<string> DefaultMappableTypes = [
        "global::System.Int32",
        "global::System.String",
        "global::System.Boolean",
        "global::System.Byte",
        "sbyte",
        "char",
        "decimal",
        "global::System.Double",
        "float",
        "int",
        "uint",
        "nint",
        "nuint",
        "long",
        "ulong",
        "short",
        "ushort",
        "global::NodaTime.Instant",
        "global::NodaTime.LocalDate",
    ];

    public static readonly List<string> PassthroughTypes =
    [
        "global::System.Threading.Tasks.Task<TResult>"
    ];

    public static readonly string ApiClientHeaders = 
         """
         import { genericPost, genericGet } from '../api-generics';
         import type { FetchType } from '../api-generics';
         import type { Dayjs } from 'dayjs';

         """;
}
