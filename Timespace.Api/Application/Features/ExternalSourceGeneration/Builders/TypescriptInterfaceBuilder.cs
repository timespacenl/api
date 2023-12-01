using System.Text;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;

public class TypescriptInterfaceSourceBuilder : ITypescriptSourceBuilder
{
    private int _indentLevel;
    private readonly StringBuilder _builder = new();
    private readonly string _indent = "    ";
    private readonly string _newLine = "\n";
    private readonly int _openScopes = 0;


    public ITypescriptSourceBuilder Initialize(string name, bool canBeNull = false)
    {
        _builder.Append($"export interface {name} {{");
        _builder.Append(_newLine);
        _indentLevel++;

        return this;
    }
    public ITypescriptSourceBuilder AddProperty(GeneratableMember member, string? typeNameOverride = null)
    {
        var listExtension = member.IsList ? "[]" : "";
        var nullableExtension = member.IsNullable ? "?" : "";
        
        var typeName = typeNameOverride;
        if (member.MemberType is not null && typeNameOverride is null)
        {
            typeName = Constants.MappableTypesMapping.TryGetValue(member.MemberType.Name, out var tsType) ? tsType : "unknown";
        }
        
        _builder.Append($"{_indent.Repeat(_indentLevel)}{member.Name.ToCamelCase()}{nullableExtension}: {typeName}{listExtension};");
        _builder.Append(_newLine);
        return this;
    }

    public string Build()
    {
        for (int i = 0; i < _openScopes - 1; i++)
        {
            _indentLevel--;
            _builder.Append($"{_indent.Repeat(_indentLevel)}}},");
            _builder.Append(_newLine);
        }
        
        _indentLevel--;
        _builder.Append($"{_indent.Repeat(_indentLevel)}}};");
        
        return _builder.ToString();
    }
}
