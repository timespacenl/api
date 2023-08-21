using System.Text;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;

public class TypescriptEnumBuilder
{
    private int _indentLevel = 0;
    private readonly StringBuilder _builder = new();
    private readonly StringBuilder _tsDocBuilder = new();
    private readonly string _indent = "    ";
    private readonly string _newLine = "\n";
    private readonly int _openScopes = 0;
    
    public TypescriptEnumBuilder(string name)
    {
        _builder.Append($"export enum {name} {{");
        _builder.Append(_newLine);
        _tsDocBuilder.AppendLine("/**");
        _indentLevel++;
    }
    
    public TypescriptEnumBuilder AddNumberEnumProperty(string name, string value)
    {
        _builder.Append($"{_indent.Repeat(_indentLevel)}{name} = {value},");
        _builder.Append(_newLine);
        
        _tsDocBuilder.AppendLine($" * @member {name} - {value}");
        return this;
    }
    
    public string Build(bool asConst = false)
    {
        _tsDocBuilder.AppendLine(" */");
        for (int i = 0; i < _openScopes - 1; i++)
        {
            _indentLevel--;
            _builder.Append($"{_indent.Repeat(_indentLevel)}}},");
            _builder.Append(_newLine);
        }
        
        _indentLevel--;
        var extension = asConst ? " as const" : "";
        _builder.Append($"{_indent.Repeat(_indentLevel)}}}{extension};");
        
        return _tsDocBuilder.ToString() + _builder;
    }
}