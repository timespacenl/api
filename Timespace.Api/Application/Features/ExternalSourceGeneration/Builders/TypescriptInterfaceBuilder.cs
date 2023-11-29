using System.Text;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;

public class TypescriptInterfaceSourceBuilder : ITypescriptSourceBuilder
{
    private int _indentLevel;
    private readonly StringBuilder _builder = new();
    private readonly string _indent = "    ";
    private readonly string _newLine = "\n";
    private readonly int _openScopes = 0;


    public ITypescriptSourceBuilder Initialize(string name)
    {
        _builder.Append($"export interface {name} {{");
        _builder.Append(_newLine);
        _indentLevel++;

        return this;
    }
    
    public ITypescriptSourceBuilder AddProperty(string name, string type, bool nullable = false, bool isList = false)
    {
        var listExtension = isList ? "[]" : "";
        var nullableExtension = nullable ? " | null" : "";
        _builder.Append($"{_indent.Repeat(_indentLevel)}{name}: {type}{listExtension}{nullableExtension};");
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
