using System.Text;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;

public class TypescriptTypeBuilder
{
    private int _indentLevel = 0;
    private readonly StringBuilder _builder = new();
    private readonly string _indent = "    ";
    private readonly string _newLine = "\n";
    private readonly int _openScopes = 0;
    
    public TypescriptTypeBuilder(string name)
    {
        _builder.Append($"export interface {name} {{");
        _builder.Append(_newLine);
        _indentLevel++;
    }
    
    public TypescriptTypeBuilder AddProperty(string name, string type, bool nullable = false, bool isList = false)
    {
        var listExtension = isList ? "[]" : "";
        var nullableExtension = nullable ? " | null" : "";
        _builder.Append($"{_indent.Repeat(_indentLevel)}{name}: {type}{listExtension}{nullableExtension};");
        _builder.Append(_newLine);
        return this;
    }
    
    public string Build(bool asConst = false)
    {
        for (int i = 0; i < _openScopes - 1; i++)
        {
            _indentLevel--;
            _builder.Append($"{_indent.Repeat(_indentLevel)}}},");
            _builder.Append(_newLine);
        }
        
        _indentLevel--;
        var extension = asConst ? " as const" : "";
        _builder.Append($"{_indent.Repeat(_indentLevel)}}}{extension};");
        
        return _builder.ToString();
    }
}