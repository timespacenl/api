using System.Text;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;

public class TypescriptToMappingBuilder : ITypescriptSourceBuilder
{
    private int _indentLevel;
    private readonly StringBuilder _builder = new();
    private readonly string _indent = "    ";
    private readonly string _newLine = "\n";
    private readonly int _openScopes = 0;
    
    public ITypescriptSourceBuilder Initialize(string name, bool canBeNull = false)
    {
        _builder.Append($"export const to{name} = (data: any): {name} => ({{");
        _builder.Append(_newLine);
        _indentLevel++;
        
        return this;
    }
    public ITypescriptSourceBuilder AddProperty(GeneratableMember member, string? typeNameOverride = null)
    {
        string propertyAccessor;
        string nullablePrefix = member.IsNullable ? $"data.{member.Name.ToCamelCase()} ? undefined : " : "";
        
        if (member.MemberType is not null && typeNameOverride is null)
        {
            if(member.MemberType == typeof(Instant) || member.MemberType == typeof(LocalDate) || member.MemberType == typeof(LocalDateTime))
                propertyAccessor = $"{nullablePrefix}dayjs(data.{member.Name.ToCamelCase()})";
            else
                propertyAccessor = $"data.{member.Name.ToCamelCase()}";
        }
        else
        {
            propertyAccessor = member.IsList ? 
                $"data.{member.Name.ToCamelCase()}{(member.IsNullable ? "?" : "")}.map((c: any) => to{typeNameOverride}(c))" : 
                $"{nullablePrefix}to{typeNameOverride}(data.{member.Name.ToCamelCase()})";
        }
        
        _builder.Append($"{_indent.Repeat(_indentLevel)}{member.Name.ToCamelCase()}: {propertyAccessor},");
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
        _builder.Append($"{_indent.Repeat(_indentLevel)}}});");
        
        return _builder.ToString();
    }
}