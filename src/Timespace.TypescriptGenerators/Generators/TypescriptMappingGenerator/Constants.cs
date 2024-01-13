using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator;

public static class Constants
{
	public static readonly IReadOnlyDictionary<string, DefaultTypeMapping> DefaultTypeMappings = new Dictionary<string, DefaultTypeMapping>
	{
		{
			"global::System.Int32", new DefaultTypeMapping("number", "z.number().int()", "z.number().int()")
		},
		{
			"global::System.Guid", new DefaultTypeMapping("string", "z.string()", "z.string()")
		},
		{
			"global::System.String", new DefaultTypeMapping("string", "z.string()", "z.string()")
		},
		{
			"global::System.Boolean", new DefaultTypeMapping("bool", "z.boolean()", "z.boolean()")
		},
		{
			"global::System.Double", new DefaultTypeMapping("number", "z.number()", "z.number()")
		},
		{
			"global::NodaTime.Instant",
			new DefaultTypeMapping("Dayjs", "z.string().transform(v => dayjs(v))",
				"z.instanceof(dayjs as unknown as typeof Dayjs).transform(v => v.toISOString())")
		},
		{
			"global::NodaTime.LocalDate",
			new DefaultTypeMapping("Dayjs", "z.string().transform(v => dayjs(v))",
				"z.instanceof(dayjs as unknown as typeof Dayjs).transform(v => v.format('YYYY-MM-DD'))")
		},
		{
			"global::Microsoft.AspNetCore.Http.IFormFile", new DefaultTypeMapping("File", "z.instanceof(File)", "z.instanceof(File)")
		},
	};

	public static readonly IReadOnlyList<string> PassthroughTypes =
	[
		"global::System.Threading.Tasks.Task<TResult>",
	];

	public static readonly string ApiClientHeaders =
		"""
         import { genericPost, genericGet } from '../api-generics';
         import type { FetchType } from '../api-generics';
         import type { Dayjs } from 'dayjs';

         """;

	private const string FetchTypeSource = "export type FetchType = typeof fetch";

	public static readonly TypescriptSourceFile FetchTypeFile = new(
		"helpers",
		"fetchType.ts",
		FetchTypeSource
	);

	private const string ConvertFormDataSource =
		"""
          function appendFormData(formData: any, data: any, rootName: any) {
          
              let root = rootName || '';
              if (data instanceof File) {
                  formData.append(root, data);
              } else if (Array.isArray(data)) {
                  for (var i = 0; i < data.length; i++) {
                      appendFormData(formData, data[i], root + '[' + i + ']');
                  }
              } else if (typeof data === 'object' && data) {
                  for (var key in data) {
                      if (data.hasOwnProperty(key)) {
                          if (root === '') {
                              appendFormData(formData, data[key], key);
                          } else {
                              appendFormData(formData, data[key], root + '.' + key);
                          }
                      }
                  }
              } else {
                  if (data !== null && typeof data !== 'undefined') {
                      formData.append(root, data);
                  }
              }
          }
          
          export function getFormData(data: unknown) {
              var formData = new FormData();
          
              appendFormData(formData, data, '');
          
              return formData;
          }
          """;

	public static readonly TypescriptSourceFile ConvertFormDataFile = new(
		"helpers",
		"getFormData.ts",
		ConvertFormDataSource
	);

	private const string ProblemDetailsSource =
		"""
        export interface IProblemDetails {
            type?: string;
            title?: string;
            status?: number;
            detail?: string;
            instance?: string;
        }
        """;

	public static readonly TypescriptSourceFile ProblemDetailsFile = new(
		"helpers",
		"problemDetails.ts",
		ProblemDetailsSource
	);
}

public record DefaultTypeMapping(string TypescriptType, string ZodToMapping, string ZodFromMapping);
