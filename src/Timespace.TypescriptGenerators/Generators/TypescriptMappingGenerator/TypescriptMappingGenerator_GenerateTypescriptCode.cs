using System.Text;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;
using TimeSpace.Shared.TypescriptGenerator;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Extensions;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator;

public partial class TypescriptMappingGenerator
{
	private List<TypescriptSourceFile> GenerateTypescriptCode(List<ApiEndpoint> endpoints)
	{
		var sourceFiles = new List<TypescriptSourceFile>();

		sourceFiles.Add(Constants.FetchTypeFile);
		sourceFiles.Add(Constants.ConvertFormDataFile);
		sourceFiles.Add(Constants.ProblemDetailsFile);

		var sharedTypes = GetSharedTypes(endpoints);
		var sharedTypesApiTypes = GetSharedApiTypes(endpoints);

		var sharedTypeSources = GenerateSharedTypes(endpoints, sharedTypes);
		sourceFiles.AddRange(sharedTypeSources);

		foreach (var endpoint in endpoints)
		{
			List<TypescriptSourceFragment> sourceFragments = new();

			if (endpoint.BodyTypeName is not null)
				sourceFragments.AddRange(
					GetChildrenFromRootTypeName(endpoint.BodyTypeName, endpoint.RequestTypes, sharedTypesApiTypes)
						.Reverse()
						.Select(x => GenerateTypescriptType(x.Value, sharedTypes, ApiTypeCategory.Request, endpoint.FormData))
						.ToList()
				);

			if (endpoint.QueryTypeName is not null)
				sourceFragments.AddRange(
					GetChildrenFromRootTypeName(endpoint.QueryTypeName, endpoint.RequestTypes, sharedTypesApiTypes)
						.Reverse()
						.Select(x => GenerateTypescriptType(x.Value, sharedTypes, ApiTypeCategory.Request, true))
						.ToList()
				);

			if (endpoint.PathTypeName is not null)
				sourceFragments.AddRange(
					GetChildrenFromRootTypeName(endpoint.PathTypeName, endpoint.RequestTypes, sharedTypesApiTypes)
						.Reverse()
						.Select(x => GenerateTypescriptType(x.Value, sharedTypes, ApiTypeCategory.Request, true))
						.ToList()
				);

			sourceFragments.AddRange(
				GetChildrenFromRootTypeName(endpoint.ResponseTypeName, endpoint.ResponseTypes, sharedTypesApiTypes)
					.Reverse()
					.Select(x => GenerateTypescriptType(x.Value, sharedTypes, ApiTypeCategory.Response))
					.ToList()
			);

			var responseTypeName = endpoint.ResponseTypes[endpoint.ResponseTypeName].TypeName;
			sourceFragments.Add(new TypescriptSourceFragment(new HashSet<TypescriptImportable>(),
				$$"""
                export function to{{endpoint.ActionName}}Response(data: unknown): {{responseTypeName}} {
                    return {{responseTypeName}}ResponseSchema.parse(data);
                }
                
                
                """));

			sourceFragments.Add(GenerateUrl(endpoint));
			sourceFragments.Add(GenerateFetchMethod(endpoint));

			sourceFiles.Add(sourceFragments.GetSourceFile($"{endpoint.RouteUrl}", $"{endpoint.ActionName.ToCamelCase()}.ts"));
		}

		return sourceFiles;
	}

	private Dictionary<string, ApiType> GetChildrenFromRootTypeName(string rootTypeName, Dictionary<string, ApiType> apiTypes,
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

	private Dictionary<string, SharedType> GetSharedTypes(List<ApiEndpoint> endpoints)
	{
		return GetSharedApiTypes(endpoints)
			.Select(x =>
			{
				if (x.Value is ApiTypeClass)
				{
					return new KeyValuePair<string, SharedType>(
						x.Key,
						new SharedType(x.Value.TypeName, $"{x.Value.TypeName}ResponseSchema", $"{x.Value.TypeName}ResponseSchemaStringified",
							$"{x.Value.TypeName}RequestSchema", $"{x.Value.TypeName}RequestSchemaStringified",
							$"$api/shared/{x.Value.TypeName.ToCamelCase()}", false));
				}
				return new KeyValuePair<string, SharedType>(
					x.Key,
					new SharedType(x.Value.TypeName, $"z.nativeEnum({x.Value.TypeName})", $"z.nativeEnum({x.Value.TypeName})", "z.number()",
						"z.number()", $"$api/enums/{x.Value.TypeName.ToCamelCase()}", true));
			})
			.ToDictionary();
	}

	private Dictionary<string, ApiType> GetSharedApiTypes(List<ApiEndpoint> endpoints)
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
		if (type is ApiTypeClass apiTypeClass)
			return GenerateTypescriptFromClassType(apiTypeClass, sharedTypes, typeCategory, generateStringified);
		if (type is ApiTypeEnum apiTypeEnum)
			return GenerateTypescriptFromEnumType(apiTypeEnum, sharedTypes);

		throw new Exception($"Unknown type {type.GetType().Name}");
	}

	private TypescriptSourceFragment GenerateTypescriptFromEnumType(ApiTypeEnum apiTypeEnum, Dictionary<string, SharedType> sharedTypes)
	{
		var sourceBuilder = new StringBuilder();
		var importables = new HashSet<TypescriptImportable>();
		if (sharedTypes.TryGetValue(apiTypeEnum.FullyQualifiedTypeName, out var sharedType))
		{
			importables.Add(new TypescriptImportable(ImportType.Type, sharedType, ImportFlags.Type));
		}
		else
		{
			sourceBuilder.Append(
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
		importables.Add(new TypescriptImportable(ImportType.Zod, null, null));
		if (typeCategory == ApiTypeCategory.Shared)
			generateStringified = true;

		var importFlags = GetImportFlags(typeCategory, generateStringified);

		if (typeCategory is ApiTypeCategory.Request or ApiTypeCategory.Response or ApiTypeCategory.Shared)
		{
			var interfaceSource = GenerateTypescriptInterface(apiTypeClass, sharedTypes, importables, importFlags);
			if (interfaceSource != "") sourceBuilder.AppendLine(interfaceSource);
		}

		if (typeCategory is ApiTypeCategory.Response or ApiTypeCategory.Shared)
		{
			if (typeCategory is ApiTypeCategory.Response && generateStringified == false || typeCategory is ApiTypeCategory.Shared)
			{
				var zodToSource = GenerateZodToType(apiTypeClass, sharedTypes, importables, typeCategory, importFlags);
				if (zodToSource != "") sourceBuilder.AppendLine(zodToSource);
			}

			if (generateStringified)
			{
				var zodToSourceStringified = GenerateZodToType(apiTypeClass, sharedTypes, importables, typeCategory, importFlags, true);
				if (zodToSourceStringified != "") sourceBuilder.AppendLine(zodToSourceStringified);
			}
		}

		if (typeCategory is ApiTypeCategory.Request or ApiTypeCategory.Shared)
		{
			if (typeCategory is ApiTypeCategory.Request && generateStringified == false || typeCategory is ApiTypeCategory.Shared)
			{
				var zodFromSource = GenerateZodFromType(apiTypeClass, sharedTypes, importables, typeCategory, importFlags);
				if (zodFromSource != "") sourceBuilder.AppendLine(zodFromSource);
			}

			if (generateStringified)
			{
				var zodFromSourceStringified = GenerateZodFromType(apiTypeClass, sharedTypes, importables, typeCategory, importFlags, true);
				if (zodFromSourceStringified != "") sourceBuilder.AppendLine(zodFromSourceStringified);
			}
		}

		return new TypescriptSourceFragment(importables, sourceBuilder.ToString());
	}

	private string GenerateTypescriptInterface(ApiTypeClass apiTypeClass, Dictionary<string, SharedType> sharedTypes,
		HashSet<TypescriptImportable> importables, ImportFlags importFlags)
	{
		var sourceBuilder = new StringBuilder();

		if (sharedTypes.TryGetValue(apiTypeClass.FullyQualifiedTypeName, out var sharedType))
		{
			importables.Add(new TypescriptImportable(ImportType.Type, sharedType, sharedType.IsEnum ? ImportFlags.Type : importFlags));
		}
		else
		{
			sourceBuilder.AppendLine($"export type {apiTypeClass.TypeName} = {{");
			foreach (var property in apiTypeClass.Properties)
			{
				if (sharedTypes.TryGetValue(property.FullyQualifiedTypeName, out var type))
				{
					importables.Add(new TypescriptImportable(ImportType.Type, type, type.IsEnum ? ImportFlags.Type : importFlags));
				}

				if (property.FullyQualifiedTypeName.Replace("?", "") is "global::NodaTime.Instant" or "global::NodaTime.LocalDate")
					importables.Add(new TypescriptImportable(ImportType.Dayjs, null, null));

				var tsType = GetMapping(property.FullyQualifiedTypeName)?.TypescriptType ?? type?.TypeName ?? property.TypeName;

				var propertyType = property.CollectionInfo.CollectionType switch
				{
					CollectionType.None => tsType + (property.IsNullable ? " | undefined" : ""),
					CollectionType.List => $"Array<{tsType}{(property.IsNullable ? " | undefined" : "")}>",
					CollectionType.Dictionary => $"Record<string, {tsType}{(property.IsNullable ? " | undefined" : "")}>",
					_ => throw new Exception($"Unknown collection type {property.CollectionInfo.CollectionType}"),
				};

				sourceBuilder.AppendLine($"    {property.Name.ToCamelCase()}{(property.IsNullable ? "?" : "")}: {propertyType},");
			}

			sourceBuilder.AppendLine("};");
		}

		return sourceBuilder.ToString();
	}

	private string GenerateZodToType(ApiTypeClass apiTypeClass, Dictionary<string, SharedType> sharedTypes, HashSet<TypescriptImportable> importables,
		ApiTypeCategory typeCategory, ImportFlags importFlags, bool generateStringified = false)
	{
		var sourceBuilder = new StringBuilder();

		if (sharedTypes.TryGetValue(apiTypeClass.FullyQualifiedTypeName, out var sharedType))
		{
			importables.Add(new TypescriptImportable(ImportType.Type, sharedType, sharedType.IsEnum ? ImportFlags.Type : importFlags));
		}
		else
		{
			sourceBuilder.AppendLine(
				$"{(typeCategory is ApiTypeCategory.Shared ? "export " : "")}const {apiTypeClass.TypeName}ResponseSchema{(generateStringified ? "Stringified" : "")} = z.object({{");
			foreach (var property in apiTypeClass.Properties)
			{
				if (sharedTypes.TryGetValue(property.FullyQualifiedTypeName, out var type))
					importables.Add(new TypescriptImportable(ImportType.Type, type, type.IsEnum ? ImportFlags.Type : importFlags));

				if (property.FullyQualifiedTypeName is "global::NodaTime.Instant" or "global::NodaTime.LocalDate")
					importables.Add(new TypescriptImportable(ImportType.Dayjs, null, null));

				var zodMapping = GetMapping(property.FullyQualifiedTypeName)?.ZodToMapping ??
					(generateStringified ? type?.RequestZodTypeStringified : type?.RequestZodType) ??
					$"{property.TypeName}ResponseSchema{(generateStringified ? "Stringified" : "")}";

				var propertyType = GetZodPropertyType(property, zodMapping, type?.IsEnum ?? false, generateStringified);

				sourceBuilder.AppendLine($"    {property.Name.ToCamelCase()}: {propertyType},");
			}

			sourceBuilder.AppendLine("});");
		}
		return sourceBuilder.ToString();
	}

	private string GenerateZodFromType(ApiTypeClass apiTypeClass, Dictionary<string, SharedType> sharedTypes,
		HashSet<TypescriptImportable> importables, ApiTypeCategory typeCategory, ImportFlags importFlags, bool generateStringified = false)
	{
		var sourceBuilder = new StringBuilder();

		if (sharedTypes.TryGetValue(apiTypeClass.FullyQualifiedTypeName, out var sharedType))
		{
			importables.Add(new TypescriptImportable(ImportType.Type, sharedType, sharedType.IsEnum ? ImportFlags.Type : importFlags));
		}
		else
		{
			sourceBuilder.AppendLine(
				$"{(typeCategory is ApiTypeCategory.Shared ? "export " : "")}const {apiTypeClass.TypeName}RequestSchema{(generateStringified ? "Stringified" : "")} = z.object({{");
			foreach (var property in apiTypeClass.Properties)
			{
				if (sharedTypes.TryGetValue(property.FullyQualifiedTypeName, out var type))
					importables.Add(new TypescriptImportable(ImportType.Type, type, type.IsEnum ? ImportFlags.Type : importFlags));

				if (property.FullyQualifiedTypeName is "global::NodaTime.Instant" or "global::NodaTime.LocalDate")
					importables.Add(new TypescriptImportable(ImportType.Dayjs, null, null));

				var zodMapping = GetMapping(property.FullyQualifiedTypeName)?.ZodFromMapping ??
					(generateStringified ? type?.ResponseZodTypeStringified : type?.ResponseZodType) ??
					$"{property.TypeName}RequestSchema{(generateStringified ? "Stringified" : "")}";

				var propertyType = GetZodPropertyType(property, zodMapping, type?.IsEnum ?? false, generateStringified);

				sourceBuilder.AppendLine($"    {property.Name.ToCamelCase()}: {propertyType},");
			}

			sourceBuilder.AppendLine("});");
		}
		return sourceBuilder.ToString();
	}

	private string GetZodPropertyType(ApiClassProperty property, string zodMapping, bool isEnum = false, bool generateStringified = false)
	{
		const string nullableExtension = ".nullish().transform(x => x ?? undefined)";
		List<string> nonStringifyableTypes =
		[
			"global::System.String",
			"global::NodaTime.Instant",
			"global::NodaTime.LocalDate",
			"global::Microsoft.AspNetCore.Http.IFormFile",
		];
		var isDefaultMappable = GetMapping(property.FullyQualifiedTypeName) == null;
		var shouldStringify = generateStringified
			&& (!isDefaultMappable && !nonStringifyableTypes.Contains(property.FullyQualifiedTypeName.Replace("?", "")) || isEnum);

		return property.CollectionInfo.CollectionType switch
		{
			CollectionType.None => zodMapping + (property.IsNullable ? nullableExtension : "")
				+ (shouldStringify ? $".transform((v: any) => v{(property.IsNullable ? "?" : "")}.toString())" : ""),
			CollectionType.List =>
				$"z.array({zodMapping}{(property.IsNullable ? nullableExtension : "")}){(property.CollectionInfo.IsNullable ? ".nullish()" : "")}",
			CollectionType.Dictionary =>
				$"z.record({zodMapping}{(property.IsNullable ? nullableExtension : "")}){(property.CollectionInfo.IsNullable ? ".nullish()" : "")}",
			_ => throw new Exception($"Unknown collection type {property.CollectionInfo.CollectionType}"),
		};
	}

	private DefaultTypeMapping? GetMapping(string typeName)
	{
		return Constants.DefaultTypeMappings.FirstOrDefault(x => x.Key == typeName.Replace("?", "")).Value;
	}
	private ImportFlags GetImportFlags(ApiTypeCategory typeCategory, bool generateStringified)
	{
		return typeCategory switch
		{
			ApiTypeCategory.Request => generateStringified
				? ImportFlags.Type | ImportFlags.ResponseStringified
				: ImportFlags.Type | ImportFlags.Response,
			ApiTypeCategory.Response => generateStringified
				? ImportFlags.Type | ImportFlags.RequestStringified
				: ImportFlags.Type | ImportFlags.Request,
			ApiTypeCategory.Shared => generateStringified
				? ImportFlags.Type | ImportFlags.Response | ImportFlags.ResponseStringified | ImportFlags.Request | ImportFlags.RequestStringified
				: ImportFlags.Type | ImportFlags.Response | ImportFlags.Request,
			_ => throw new Exception($"Unknown type category {typeCategory}"),
		};
	}

	private TypescriptSourceFragment GenerateUrl(ApiEndpoint endpoint)
	{
		var pathParameters = endpoint.Parameters.Where(x => x.Source == ParameterSource.Path).ToList();
		var requestTypes = endpoint.RequestTypes;
		var routeUrl = endpoint.RouteUrl;

		var sourceBuilder = new StringBuilder();

		var queryParamTypeName = requestTypes.FirstOrDefault(x => x.Key == endpoint.QueryTypeName).Value;
		var pathParamTypeName = requestTypes.FirstOrDefault(x => x.Key == endpoint.PathTypeName).Value;

		Dictionary<string, string> methodParams = new();
		if (queryParamTypeName != null)
			methodParams.Add("query", queryParamTypeName.TypeName);
		if (pathParamTypeName != null)
			methodParams.Add("path", pathParamTypeName.TypeName);

		sourceBuilder.AppendLine(
			$"export const {endpoint.ActionName.ToCamelCase()}Url = ({string.Join(", ", methodParams.Select(x => $"{x.Key}: {x.Value}"))}): string => {{");

		if (pathParamTypeName is not null)
		{
			sourceBuilder.AppendLine($$"""
                                           const pathParams = {{pathParamTypeName.TypeName}}RequestSchemaStringified.parse(path);
                                            
                                       """);

			foreach (var pathParameter in pathParameters)
			{
				routeUrl = routeUrl.Replace($"{{{pathParameter.Name}}}", $"${{pathParams.{pathParameter.Name.ToCamelCase()}}}");
			}
		}

		if (queryParamTypeName is not null)
		{
			sourceBuilder.AppendLine($$"""
                                           const queryParams = {{queryParamTypeName.TypeName}}RequestSchemaStringified.parse(query);
                                           const urlSearchParams = new URLSearchParams(queryParams);
                                            
                                       """);

			routeUrl = $"{routeUrl}?${{urlSearchParams.toString()}}";
		}

		sourceBuilder.AppendLine($"    return `${{PUBLIC_BASE_URL}}{routeUrl}`;");
		sourceBuilder.AppendLine("};");
		sourceBuilder.AppendLine();

		return new TypescriptSourceFragment(new HashSet<TypescriptImportable>(), sourceBuilder.ToString());
	}

	private TypescriptSourceFragment GenerateFetchMethod(ApiEndpoint endpoint)
	{
		var importables = new HashSet<TypescriptImportable>();
		importables.Add(new TypescriptImportable(ImportType.FetchType, null, null));
		importables.Add(new TypescriptImportable(ImportType.ProblemDetails, null, null));
		importables.Add(new TypescriptImportable(ImportType.BaseUrl, null, null));
		if (endpoint.FormData)
			importables.Add(new TypescriptImportable(ImportType.Formdata, null, null));

		var endpointTypes = endpoint.RequestTypes.Concat(endpoint.ResponseTypes).ToDictionary();

		Dictionary<string, string> methodParams = new();
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

		sourceBuilder.AppendLine(func);

		return new TypescriptSourceFragment(importables, sourceBuilder.ToString());
	}

	private string GenerateGet()
	{
		return """
                   const response = await fetch(url, {
                       method: 'GET'
                   });
               """;
	}

	private string GeneratePostLike(string method, string bodyTypeName, bool formData)
	{
		if (method is not "POST" and not "PUT" and not "PATCH")
			throw new ArgumentException("Method must be POST, PUT or PATCH");

		if (formData)
		{
			return $$"""
                       const bodyObj = {{bodyTypeName}}RequestSchemaStringified.parse(body);
                       const bodyData = getFormData(bodyObj);
                       
                       const response = await fetch(url, {
                           method: '{{method}}',
                           headers: {
                               'Content-Type': 'multipart/form-data'
                           },
                           body: bodyData
                       });
                   """;
		}

		return $$"""
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

	private string GenerateDelete()
	{
		return """
               const response = await fetch(url, {
                   method: 'DELETE'
               });
               """;
	}
}
