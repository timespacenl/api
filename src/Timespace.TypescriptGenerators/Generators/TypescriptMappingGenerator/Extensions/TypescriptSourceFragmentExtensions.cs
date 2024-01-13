using System.Text;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Extensions;

internal static class TypescriptSourceFragmentExtensions
{
	public static TypescriptSourceFile GetSourceFile(this List<TypescriptSourceFragment> fragments, string filePath, string fileName)
	{
		var stringBuilder = new StringBuilder();
		var imports = fragments.SelectMany(x => x.Imports).DistinctBy(x => new
		{
			x.ImportType,
			x.ImportableType,
		}).ToHashSet();
		var importBuilder = new StringBuilder();
		if (imports.Any(x => x.ImportType == ImportType.Zod))
			_ = importBuilder.AppendLine("import { z } from 'zod';");
		if (imports.Any(x => x.ImportType == ImportType.Dayjs))
			_ = importBuilder.AppendLine("import dayjs, { type Dayjs } from 'dayjs';");
		if (imports.Any(x => x.ImportType == ImportType.Formdata))
			_ = importBuilder.AppendLine("import { getFormData } from '$api/helpers/getFormData';");
		if (imports.Any(x => x.ImportType == ImportType.FetchType))
			_ = importBuilder.AppendLine("import type { FetchType } from '$api/helpers/fetchType';");
		if (imports.Any(x => x.ImportType == ImportType.ProblemDetails))
			_ = importBuilder.AppendLine("import type { IProblemDetails } from '$api/helpers/problemDetails';");
		if (imports.Any(x => x.ImportType == ImportType.BaseUrl))
			_ = importBuilder.AppendLine("import { PUBLIC_BASE_URL } from '$env/static/public';");

		if (imports.Any(x => x.ImportableType is not null))
		{
			foreach (var importableType in imports
				.Where(x => x.ImportType == ImportType.Type && x is { ImportableType: not null, ImportFlags: not null }).ToList())
			{
				var fromImport = importableType.ImportFlags is not null && importableType.ImportFlags.Value.HasFlag(ImportableTypes.Response)
					? $", {importableType.ImportableType!.ResponseZodType}"
					: "";
				var toImport = importableType.ImportFlags is not null && importableType.ImportFlags.Value.HasFlag(ImportableTypes.Request)
					? $", {importableType.ImportableType!.RequestZodType}"
					: "";
				var fromImportStringified =
					importableType.ImportFlags is not null && importableType.ImportFlags.Value.HasFlag(ImportableTypes.ResponseStringified)
						? $", {importableType.ImportableType!.ResponseZodTypeStringified}"
						: "";
				var toImportStringified =
					importableType.ImportFlags is not null && importableType.ImportFlags.Value.HasFlag(ImportableTypes.RequestStringified)
						? $", {importableType.ImportableType!.RequestZodTypeStringified}"
						: "";


				var typeImportString =
					$"import {{ {(!importableType.ImportableType!.IsEnum ? "type " : "")}{importableType.ImportableType!.TypeName}{fromImport}{toImport}{fromImportStringified}{toImportStringified} }} from '{importableType.ImportableType!.ImportPath}';";
				_ = importBuilder.AppendLine(typeImportString);
			}
		}

		if (importBuilder.Length > 0)
		{
			_ = stringBuilder.Append(importBuilder);
			_ = stringBuilder.Append('\n');
		}

		foreach (var fragment in fragments)
		{
			_ = stringBuilder.Append(fragment.TypeSource);
		}

		return new TypescriptSourceFile(filePath, fileName, stringBuilder.ToString());
	}
}
