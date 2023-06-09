using System.Text;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;

public class TypescriptObjectBuilder
{
    private int _indentLevel = 0;
    private readonly StringBuilder _builder = new();
    private readonly string _indent = "    ";
    private readonly string _newLine = "\n";
    private readonly int _openScopes = 0;
    
    public TypescriptObjectBuilder(string name)
    {
        _builder.Append($"export const {name} = {{");
        _builder.Append(_newLine);
        _indentLevel++;
    }
    
    public TypescriptObjectBuilder OpenScope(string name)
    {
        _builder.Append($"{_indent.Repeat(_indentLevel)}{name}: {{");
        _builder.Append(_newLine);
        _indentLevel++;
        return this;
    }
    
    public TypescriptObjectBuilder CloseScope()
    {
        _indentLevel--;
        _builder.Append($"{_indent.Repeat(_indentLevel)}}},");
        _builder.Append(_newLine);
        return this;
    }
    
    public TypescriptObjectBuilder AddProperty(string name, string value)
    {
        _builder.Append($"{_indent.Repeat(_indentLevel)}{name}: \"{value}\",");
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