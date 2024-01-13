using System.Diagnostics.CodeAnalysis;
using System.Text;
using TimeSpace.Shared.TypescriptGenerator;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Extensions;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator;

[SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
internal partial class TypescriptMappingGenerator
{
	private List<TypescriptSourceFile> GenerateTypescriptCode(List<ApiEndpoint> endpoints)
	{
		var sourceFiles = new List<TypescriptSourceFile>
		{
			Constants.FetchTypeFile,
			Constants.ConvertFormDataFile,
			Constants.ProblemDetailsFile,
		};

		var sharedTypes = GetSharedTypes(endpoints);
		var sharedTypesApiTypes = GetSharedApiTypes(endpoints);

		var sharedTypeSources = GenerateSharedTypes(endpoints, sharedTypes);
		sourceFiles.AddRange(sharedTypeSources);

		foreach (var endpoint in endpoints)
		{
			List<TypescriptSourceFragment> sourceFragments = [];

			if (endpoint.BodyTypeName is not null)
			{
				sourceFragments.AddRange(
					GetChildrenFromRootTypeName(endpoint.BodyTypeName, endpoint.RequestTypes, sharedTypesApiTypes)
						.Reverse()
						.Select(x => GenerateTypescriptType(x.Value, sharedTypes, ApiTypeCategory.Request, endpoint.FormData))
						.ToList()
				);
			}

			if (endpoint.QueryTypeName is not null)
			{
				sourceFragments.AddRange(
					GetChildrenFromRootTypeName(endpoint.QueryTypeName, endpoint.RequestTypes, sharedTypesApiTypes)
						.Reverse()
						.Select(x => GenerateTypescriptType(x.Value, sharedTypes, ApiTypeCategory.Request, true))
						.ToList()
				);
			}

			if (endpoint.PathTypeName is not null)
			{
				sourceFragments.AddRange(
					GetChildrenFromRootTypeName(endpoint.PathTypeName, endpoint.RequestTypes, sharedTypesApiTypes)
						.Reverse()
						.Select(x => GenerateTypescriptType(x.Value, sharedTypes, ApiTypeCategory.Request, true))
						.ToList()
				);
			}

			sourceFragments.AddRange(
				GetChildrenFromRootTypeName(endpoint.ResponseTypeName, endpoint.ResponseTypes, sharedTypesApiTypes)
					.Reverse()
					.Select(x => GenerateTypescriptType(x.Value, sharedTypes, ApiTypeCategory.Response))
					.ToList()
			);

			var responseTypeName = endpoint.ResponseTypes[endpoint.ResponseTypeName].TypeName;
			sourceFragments.Add(new TypescriptSourceFragment([
				],
				$$"""
                export function to{{endpoint.ActionName}}Response(data: unknown): {{responseTypeName}} {
                    return {{responseTypeName}}ResponseSchema.parse(data);
                }
                
                
                """));

			sourceFragments.Add(GenerateUrl(endpoint));
			sourceFragments.Add(GenerateFetchMethod(endpoint));

			sourceFiles.Add(sourceFragments.GetSourceFile($"{endpoint.Route}", $"{endpoint.ActionName.ToCamelCase()}.ts"));
		}

		return sourceFiles;
	}

	private static Dictionary<string, ApiType> GetChildrenFromRootTypeName(string rootTypeName, Dictionary<string, ApiType> apiTypes,
		Dictionary<string, ApiType> sharedTypes)
	{
		var children = new Dictionary<string, ApiType>();
		var apiType = apiTypes.FirstOrDefault(x => x.Value is ApiTypeClass apiTypeClass && apiTypeClass.FullyQualifiedTypeName == rootTypeName);
		if (apiType.Value is null)
			throw new Exception($"Could not find root type {rootTypeName}");
		children.Add(apiType.Key, apiType.Value);

		if (!sharedTypes.ContainsKey(apiType.Value.FullyQualifiedTypeName)) GetChildren(apiType.Value);

		return children;

		void GetChildren(ApiType parent)
		{
			if (parent is not ApiTypeClass apiTypeClass)
				return;

			foreach (var property in apiTypeClass.Properties)
			{
				if (apiTypes.TryGetValue(property.FullyQualifiedTypeName, out var type))
				{
					var isSharedType = sharedTypes.ContainsKey(property.FullyQualifiedTypeName);
					_ = children.TryAdd(property.FullyQualifiedTypeName, type);
					if (!isSharedType) GetChildren(type);
				}
			}
		}
	}

	private List<TypescriptSourceFile> GenerateSharedTypes(List<ApiEndpoint> endpoints, Dictionary<string, SharedType> sharedTypes)
	{
		var sourceFiles = new List<TypescriptSourceFile>();

		var sharedApiTypes = GetSharedApiTypes(endpoints);

		foreach (var sharedType in sharedApiTypes)
		{
			var sourceFragments = new List<TypescriptSourceFragment>();
			var localSharedTypes = sharedTypes.Where(x => x.Key != sharedType.Key).ToDictionary();

			sourceFragments.Add(GenerateTypescriptType(sharedType.Value, localSharedTypes, ApiTypeCategory.Shared));

			// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
			if (sharedType.Value is ApiTypeClass)
				sourceFiles.Add(sourceFragments.GetSourceFile("shared", $"{sharedType.Value.TypeName.ToCamelCase()}.ts"));
			else
				sourceFiles.Add(sourceFragments.GetSourceFile("enums", $"{sharedType.Value.TypeName.ToCamelCase()}.ts"));
		}

		return sourceFiles;
	}

	private static Dictionary<string, SharedType> GetSharedTypes(List<ApiEndpoint> endpoints)
	{
		return GetSharedApiTypes(endpoints)
			.Select(x => x.Value is ApiTypeClass
				? new KeyValuePair<string, SharedType>(
					x.Key,
					new SharedType(x.Value.TypeName, $"{x.Value.TypeName}ResponseSchema", $"{x.Value.TypeName}ResponseSchemaStringified",
						$"{x.Value.TypeName}RequestSchema", $"{x.Value.TypeName}RequestSchemaStringified",
						$"$api/shared/{x.Value.TypeName.ToCamelCase()}", false))
				: new KeyValuePair<string, SharedType>(
					x.Key,
					new SharedType(x.Value.TypeName, $"z.nativeEnum({x.Value.TypeName})", $"z.nativeEnum({x.Value.TypeName})", "z.number()",
						"z.number()", $"$api/enums/{x.Value.TypeName.ToCamelCase()}", true)))
			.ToDictionary();
	}

	private static Dictionary<string, ApiType> GetSharedApiTypes(List<ApiEndpoint> endpoints)
	{
		var allTypes = endpoints.SelectMany(x => x.RequestTypes.Concat(x.ResponseTypes)).ToList();
		var allTypeNames = endpoints.SelectMany(x => x.RequestTypes.Keys.ToList().Concat(x.ResponseTypes.Keys.ToList()));
		var sharedTypeNames = allTypeNames.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
		return allTypes
			.Where(x => sharedTypeNames.Contains(x.Key) || x.Value is ApiTypeEnum)
			.DistinctBy(x => x.Key)
			.ToDictionary();
	}

	private TypescriptSourceFragment GenerateTypescriptType(ApiType type, Dictionary<string, SharedType> sharedTypes, ApiTypeCategory typeCategory,
		bool generateStringified = false)
	{
		return type switch
		{
			ApiTypeClass apiTypeClass => GenerateTypescriptFromClassType(apiTypeClass, sharedTypes, typeCategory, generateStringified),
			ApiTypeEnum apiTypeEnum => GenerateTypescriptFromEnumType(apiTypeEnum, sharedTypes),
			_ => throw new Exception($"Unknown type {type.GetType().Name}"),
		};

	}

	private static TypescriptSourceFragment GenerateTypescriptFromEnumType(ApiTypeEnum apiTypeEnum, Dictionary<string, SharedType> sharedTypes)
	{
		var sourceBuilder = new StringBuilder();
		var importables = new HashSet<TypescriptImportable>();
		if (sharedTypes.TryGetValue(apiTypeEnum.FullyQualifiedTypeName, out var sharedType))
		{
			_ = importables.Add(new TypescriptImportable(ImportType.Type, sharedType, ImportableTypes.Type));
		}
		else
		{
			_ = sourceBuilder.Append(
				$$"""
                  export enum {{apiTypeEnum.TypeName}} {
                  {{string.Join("\n", apiTypeEnum.Values.Select(x => $"     {x.Name} = {x.Value},"))}}
                  }
                  """);
		}

		return new TypescriptSourceFragment(importables, sourceBuilder.ToString());
	}

	private TypescriptSourceFragment GenerateTypescriptFromClassType(ApiTypeClass apiTypeClass, Dictionary<string, SharedType> sharedTypes,
		ApiTypeCategory typeCategory, bool generateStringified)
	{
		var importables = new HashSet<TypescriptImportable>();
		var sourceBuilder = new StringBuilder();
		_ = importables.Add(new TypescriptImportable(ImportType.Zod, null, null));
		if (typeCategory == ApiTypeCategory.Shared)
			generateStringified = true;

		var importFlags = GetImportFlags(typeCategory, generateStringified);

		if (typeCategory is ApiTypeCategory.Request or ApiTypeCategory.Response or ApiTypeCategory.Shared)
		{
			var interfaceSource = GenerateTypescriptInterface(apiTypeClass, sharedTypes, importables, importFlags);
			if (!string.IsNullOrEmpty(interfaceSource)) _ = sourceBuilder.AppendLine(interfaceSource);
		}

		if (typeCategory is ApiTypeCategory.Response or ApiTypeCategory.Shared)
		{
			if ((typeCategory is ApiTypeCategory.Response && generateStringified == false) || typeCategory is ApiTypeCategory.Shared)
			{
				var zodToSource = GenerateZodToType(apiTypeClass, sharedTypes, importables, typeCategory, importFlags);
				if (!string.IsNullOrEmpty(zodToSource)) _ = sourceBuilder.AppendLine(zodToSource);
			}

			if (generateStringified)
			{
				var zodToSourceStringified = GenerateZodToType(apiTypeClass, sharedTypes, importables, typeCategory, importFlags, true);
				if (!string.IsNullOrEmpty(zodToSourceStringified)) _ = sourceBuilder.AppendLine(zodToSourceStringified);
			}
		}

		if (typeCategory is ApiTypeCategory.Request or ApiTypeCategory.Shared)
		{
			if ((typeCategory is ApiTypeCategory.Request && generateStringified == false) || typeCategory is ApiTypeCategory.Shared)
			{
				var zodFromSource = GenerateZodFromType(apiTypeClass, sharedTypes, importables, typeCategory, importFlags);
				if (!string.IsNullOrEmpty(zodFromSource)) _ = sourceBuilder.AppendLine(zodFromSource);
			}

			if (generateStringified)
			{
				var zodFromSourceStringified = GenerateZodFromType(apiTypeClass, sharedTypes, importables, typeCategory, importFlags, true);
				if (!string.IsNullOrEmpty(zodFromSourceStringified)) _ = sourceBuilder.AppendLine(zodFromSourceStringified);
			}
		}

		return new TypescriptSourceFragment(importables, sourceBuilder.ToString());
	}

	private static string GenerateTypescriptInterface(ApiTypeClass apiTypeClass, Dictionary<string, SharedType> sharedTypes,
		HashSet<TypescriptImportable> importables, ImportableTypes importableTypes)
	{
		var sourceBuilder = new StringBuilder();

		if (sharedTypes.TryGetValue(apiTypeClass.FullyQualifiedTypeName, out var sharedType))
		{
			_ = importables.Add(new TypescriptImportable(ImportType.Type, sharedType, sharedType.IsEnum ? ImportableTypes.Type : importableTypes));
		}
		else
		{
			_ = sourceBuilder.AppendLine($"export type {apiTypeClass.TypeName} = {{");
			foreach (var property in apiTypeClass.Properties)
			{
				if (sharedTypes.TryGetValue(property.FullyQualifiedTypeName, out var type))
				{
					_ = importables.Add(new TypescriptImportable(ImportType.Type, type, type.IsEnum ? ImportableTypes.Type : importableTypes));
				}

				if (property.FullyQualifiedTypeName.Replace("?", "", StringComparison.InvariantCultureIgnoreCase) is "global::NodaTime.Instant" or "global::NodaTime.LocalDate")
					_ = importables.Add(new TypescriptImportable(ImportType.Dayjs, null, null));

				var tsType = GetMapping(property.FullyQualifiedTypeName)?.TypescriptType ?? type?.TypeName ?? property.TypeName;

				var propertyType = property.CollectionInfo.CollectionType switch
				{
					CollectionType.None => tsType + (property.IsNullable ? " | undefined" : ""),
					CollectionType.List => $"Array<{tsType}{(property.IsNullable ? " | undefined" : "")}>",
					CollectionType.Dictionary => $"Record<string, {tsType}{(property.IsNullable ? " | undefined" : "")}>",
					_ => throw new Exception($"Unknown collection type {property.CollectionInfo.CollectionType}"),
				};

				_ = sourceBuilder.AppendLine($"    {property.Name.ToCamelCase()}{(property.IsNullable ? "?" : "")}: {propertyType},");
			}

			_ = sourceBuilder.AppendLine("};");
		}

		return sourceBuilder.ToString();
	}

	private string GenerateZodToType(ApiTypeClass apiTypeClass, Dictionary<string, SharedType> sharedTypes, HashSet<TypescriptImportable> importables,
		ApiTypeCategory typeCategory, ImportableTypes importableTypes, bool generateStringified = false)
	{
		var sourceBuilder = new StringBuilder();

		if (sharedTypes.TryGetValue(apiTypeClass.FullyQualifiedTypeName, out var sharedType))
		{
			_ = importables.Add(new TypescriptImportable(ImportType.Type, sharedType, sharedType.IsEnum ? ImportableTypes.Type : importableTypes));
		}
		else
		{
			_ = sourceBuilder.AppendLine(
				$"{(typeCategory is ApiTypeCategory.Shared ? "export " : "")}const {apiTypeClass.TypeName}ResponseSchema{(generateStringified ? "Stringified" : "")} = z.object({{");
			foreach (var property in apiTypeClass.Properties)
			{
				if (sharedTypes.TryGetValue(property.FullyQualifiedTypeName, out var type))
					_ = importables.Add(new TypescriptImportable(ImportType.Type, type, type.IsEnum ? ImportableTypes.Type : importableTypes));

				if (property.FullyQualifiedTypeName is "global::NodaTime.Instant" or "global::NodaTime.LocalDate")
					_ = importables.Add(new TypescriptImportable(ImportType.Dayjs, null, null));

				var zodMapping = GetMapping(property.FullyQualifiedTypeName)?.ZodToMapping ??
					(generateStringified ? type?.RequestZodTypeStringified : type?.RequestZodType) ??
					$"{property.TypeName}ResponseSchema{(generateStringified ? "Stringified" : "")}";

				var propertyType = GetZodPropertyType(property, zodMapping, type?.IsEnum ?? false, generateStringified);

				_ = sourceBuilder.AppendLine($"    {property.Name.ToCamelCase()}: {propertyType},");
			}

			_ = sourceBuilder.AppendLine("});");
		}
		return sourceBuilder.ToString();
	}

	private string GenerateZodFromType(ApiTypeClass apiTypeClass, Dictionary<string, SharedType> sharedTypes,
		HashSet<TypescriptImportable> importables, ApiTypeCategory typeCategory, ImportableTypes importableTypes, bool generateStringified = false)
	{
		var sourceBuilder = new StringBuilder();

		if (sharedTypes.TryGetValue(apiTypeClass.FullyQualifiedTypeName, out var sharedType))
		{
			_ = importables.Add(new TypescriptImportable(ImportType.Type, sharedType, sharedType.IsEnum ? ImportableTypes.Type : importableTypes));
		}
		else
		{
			_ = sourceBuilder.AppendLine(
				$"{(typeCategory is ApiTypeCategory.Shared ? "export " : "")}const {apiTypeClass.TypeName}RequestSchema{(generateStringified ? "Stringified" : "")} = z.object({{");
			foreach (var property in apiTypeClass.Properties)
			{
				if (sharedTypes.TryGetValue(property.FullyQualifiedTypeName, out var type))
					_ = importables.Add(new TypescriptImportable(ImportType.Type, type, type.IsEnum ? ImportableTypes.Type : importableTypes));

				if (property.FullyQualifiedTypeName is "global::NodaTime.Instant" or "global::NodaTime.LocalDate")
					_ = importables.Add(new TypescriptImportable(ImportType.Dayjs, null, null));

				var zodMapping = GetMapping(property.FullyQualifiedTypeName)?.ZodFromMapping ??
					(generateStringified ? type?.ResponseZodTypeStringified : type?.ResponseZodType) ??
					$"{property.TypeName}RequestSchema{(generateStringified ? "Stringified" : "")}";

				var propertyType = GetZodPropertyType(property, zodMapping, type?.IsEnum ?? false, generateStringified);

				_ = sourceBuilder.AppendLine($"    {property.Name.ToCamelCase()}: {propertyType},");
			}

			_ = sourceBuilder.AppendLine("});");
		}
		return sourceBuilder.ToString();
	}

	private string GetZodPropertyType(ApiClassProperty property, string zodMapping, bool isEnum = false, bool generateStringified = false)
	{
		const string NullableExtension = ".nullish().transform(x => x ?? undefined)";
		List<string> nonStringifyableTypes =
		[
			"global::System.String",
			"global::NodaTime.Instant",
			"global::NodaTime.LocalDate",
			"global::Microsoft.AspNetCore.Http.IFormFile",
		];
		var isDefaultMappable = GetMapping(property.FullyQualifiedTypeName) == null;
		var shouldStringify = generateStringified
			&& ((!isDefaultMappable && !nonStringifyableTypes.Contains(property.FullyQualifiedTypeName.Replace("?", "", StringComparison.InvariantCultureIgnoreCase))) || isEnum);

		return property.CollectionInfo.CollectionType switch
		{
			CollectionType.None => zodMapping + (property.IsNullable ? NullableExtension : "")
				+ (shouldStringify ? $".transform((v: any) => v{(property.IsNullable ? "?" : "")}.toString())" : ""),
			CollectionType.List =>
				$"z.array({zodMapping}{(property.IsNullable ? NullableExtension : "")}){(property.CollectionInfo.IsNullable ? ".nullish()" : "")}",
			CollectionType.Dictionary =>
				$"z.record({zodMapping}{(property.IsNullable ? NullableExtension : "")}){(property.CollectionInfo.IsNullable ? ".nullish()" : "")}",
			_ => throw new Exception($"Unknown collection type {property.CollectionInfo.CollectionType}"),
		};
	}

	private static DefaultTypeMapping? GetMapping(string typeName)
	{
		return Constants.DefaultTypeMappings.FirstOrDefault(x => x.Key == typeName.Replace("?", "", StringComparison.InvariantCultureIgnoreCase)).Value;
	}
	private static ImportableTypes GetImportFlags(ApiTypeCategory typeCategory, bool generateStringified)
	{
		return typeCategory switch
		{
			ApiTypeCategory.Request => generateStringified
				? ImportableTypes.Type | ImportableTypes.ResponseStringified
				: ImportableTypes.Type | ImportableTypes.Response,
			ApiTypeCategory.Response => generateStringified
				? ImportableTypes.Type | ImportableTypes.RequestStringified
				: ImportableTypes.Type | ImportableTypes.Request,
			ApiTypeCategory.Shared => generateStringified
				? ImportableTypes.Type | ImportableTypes.Response | ImportableTypes.ResponseStringified | ImportableTypes.Request | ImportableTypes.RequestStringified
				: ImportableTypes.Type | ImportableTypes.Response | ImportableTypes.Request,
			_ => throw new Exception($"Unknown type category {typeCategory}"),
		};
	}

	private static TypescriptSourceFragment GenerateUrl(ApiEndpoint endpoint)
	{
		var pathParameters = endpoint.Parameters.Where(x => x.Source == ParameterSource.Path).ToList();
		var requestTypes = endpoint.RequestTypes;
		var routeUrl = endpoint.Route;

		var sourceBuilder = new StringBuilder();

		var queryParamTypeName = requestTypes.FirstOrDefault(x => x.Key == endpoint.QueryTypeName).Value;
		var pathParamTypeName = requestTypes.FirstOrDefault(x => x.Key == endpoint.PathTypeName).Value;

		Dictionary<string, string> methodParams = [];
		if (queryParamTypeName != null)
			methodParams.Add("query", queryParamTypeName.TypeName);
		if (pathParamTypeName != null)
			methodParams.Add("path", pathParamTypeName.TypeName);

		_ = sourceBuilder.AppendLine(
			$"export const {endpoint.ActionName.ToCamelCase()}Url = ({string.Join(", ", methodParams.Select(x => $"{x.Key}: {x.Value}"))}): string => {{");

		if (pathParamTypeName is not null)
		{
			_ = sourceBuilder.AppendLine($$"""
                                           const pathParams = {{pathParamTypeName.TypeName}}RequestSchemaStringified.parse(path);
                                            
                                       """);

			foreach (var pathParameter in pathParameters)
			{
				routeUrl = routeUrl.Replace($"{{{pathParameter.Name}}}", $"${{pathParams.{pathParameter.Name.ToCamelCase()}}}", StringComparison.InvariantCulture);
			}
		}

		if (queryParamTypeName is not null)
		{
			_ = sourceBuilder.AppendLine($$"""
                                           const queryParams = {{queryParamTypeName.TypeName}}RequestSchemaStringified.parse(query);
                                           const urlSearchParams = new URLSearchParams(queryParams);
                                            
                                       """);

			routeUrl = $"{routeUrl}?${{urlSearchParams.toString()}}";
		}

		_ = sourceBuilder.AppendLine($"    return `${{PUBLIC_BASE_URL}}{routeUrl}`;");
		_ = sourceBuilder.AppendLine("};");
		_ = sourceBuilder.AppendLine();

		return new TypescriptSourceFragment([], sourceBuilder.ToString());
	}

	private TypescriptSourceFragment GenerateFetchMethod(ApiEndpoint endpoint)
	{
		var importables = new HashSet<TypescriptImportable>();
		_ = importables.Add(new TypescriptImportable(ImportType.FetchType, null, null));
		_ = importables.Add(new TypescriptImportable(ImportType.ProblemDetails, null, null));
		_ = importables.Add(new TypescriptImportable(ImportType.BaseUrl, null, null));
		if (endpoint.FormData)
			_ = importables.Add(new TypescriptImportable(ImportType.Formdata, null, null));

		var endpointTypes = endpoint.RequestTypes.Concat(endpoint.ResponseTypes).ToDictionary();

		Dictionary<string, string> methodParams = [];
		if (endpoint.QueryTypeName is not null)
			methodParams.Add("query", endpointTypes[endpoint.QueryTypeName].TypeName);
		if (endpoint.PathTypeName is not null)
			methodParams.Add("path", endpointTypes[endpoint.PathTypeName].TypeName);
		if (endpoint.BodyTypeName is not null)
			methodParams.Add("body", endpointTypes[endpoint.BodyTypeName].TypeName);

		var sourceBuilder = new StringBuilder();

		var fetchFunction = endpoint.HttpMethod switch
		{
			"GET" => GenerateGet(),
			"DELETE" => GenerateDelete(),
			"POST" => GeneratePostLike(endpoint.HttpMethod,
				endpointTypes[endpoint.BodyTypeName ?? throw new Exception("POST method should have body type")].TypeName, endpoint.FormData),
			"PUT" => GeneratePostLike(endpoint.HttpMethod,
				endpointTypes[endpoint.BodyTypeName ?? throw new Exception("PUT method should have body type")].TypeName, endpoint.FormData),
			"PATCH" => GeneratePostLike(endpoint.HttpMethod,
				endpointTypes[endpoint.BodyTypeName ?? throw new Exception("PATCH method should have body type")].TypeName, endpoint.FormData),

			_ => throw new Exception($"Unknown http method {endpoint.HttpMethod}"),
		};

		var func = $$"""
                     export const {{endpoint.ActionName.ToCamelCase()}} = async (
                         fetch: FetchType,
                     {{string.Join(",\n", methodParams.Select(x => $"    {x.Key}: {x.Value}"))}}
                     ): Promise<{{endpointTypes[endpoint.ResponseTypeName].TypeName}}> => {
                         let url = {{endpoint.ActionName.ToCamelCase()}}Url({{string.Join(",", methodParams.Where(x => x.Key != "body").Select(x => x.Key))}});
                     {{fetchFunction}}
                     
                         if (!response.ok) {
                             throw await response.json() as IProblemDetails;
                         }
                     
                         return to{{endpoint.ActionName}}Response(await response.json());
                     };
                     """;

		_ = sourceBuilder.AppendLine(func);

		return new TypescriptSourceFragment(importables, sourceBuilder.ToString());
	}

	private static string GenerateGet()
	{
		return """
                   const response = await fetch(url, {
                       method: 'GET'
                   });
               """;
	}

	private static string GeneratePostLike(string method, string bodyTypeName, bool formData)
	{
		return method is not "POST" and not "PUT" and not "PATCH"
			? throw new ArgumentException("Method must be POST, PUT or PATCH")
			: formData ? $$"""
                       const bodyObj = {{bodyTypeName}}RequestSchemaStringified.parse(body);
                       const bodyData = getFormData(bodyObj);
                       
                       const response = await fetch(url, {
                           method: '{{method}}',
                           headers: {
                               'Content-Type': 'multipart/form-data'
                           },
                           body: bodyData
                       });
                   """ : $$"""
                   const bodyObj = {{bodyTypeName}}RequestSchema.parse(body);
                   
                   const response = await fetch(url, {
                       method: '{{method}}',
                       headers: {
                           'Content-Type': 'application/json'
                       },
                       body: JSON.stringify(bodyObj)
                   });
               """;
	}

	private static string GenerateDelete()
	{
		return """
               const response = await fetch(url, {
                   method: 'DELETE'
               });
               """;
	}
}
