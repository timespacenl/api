using System.Text;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;

public class ZodSchemaBuilder
{
    private StringBuilder _builder = new StringBuilder();
    private readonly string _indent = "    ";
    private int _indentLevel;
    private int _openZObjectScopes;
    private bool _openPropertyScope;
    private int _validatorAmount;
    
    public string Build()
    {
        while (_openZObjectScopes > 0)
        {
            _indentLevel--;
            if (_openZObjectScopes == 1)
            {
                _builder.AppendLine($"{_indent.Repeat(_indentLevel)}}});");
            }
            else
            {
                _builder.AppendLine($"{_indent.Repeat(_indentLevel)}}}),");
            }
            _openZObjectScopes--;
        }
        
        if(_openPropertyScope)
            throw new Exception("There are still open property scopes.");

        return _builder.ToString();
    }

    public ZodSchemaBuilder(string name)
    {
        _builder.AppendLine($"{_indent.Repeat(_indentLevel)}export const {name} = z.object({{");
        _indentLevel++;
        _openZObjectScopes++;
    }

    public ZodSchemaBuilder OpenZObjectScope(string name)
    {
        if(_openPropertyScope)
            throw new Exception("Cannot open a z.object scope while there are still open property scopes.");
        
        _builder.AppendLine($"{_indent.Repeat(_indentLevel)}{name}: z.object({{");
        _indentLevel++;
        _openZObjectScopes++;
        return this;
    }
    
    public ZodSchemaBuilder CloseZObjectScope()
    {
        if(_openZObjectScopes == 0)
            throw new Exception("Cannot close a z.object scope while there are no open z.object scopes.");
        
        if(_openPropertyScope)
            throw new Exception("Cannot close a z.object scope while there are still open property scopes.");
        
        _indentLevel--;
        _builder.AppendLine($"{_indent.Repeat(_indentLevel)}}}),");
        _openZObjectScopes--;
        return this;
    }
    
    public ZodSchemaBuilder OpenZPropertyScope(string name, string type)
    {
        if(_openPropertyScope)
            throw new Exception("Cannot open a property scope while there are still open property scopes.");
        
        _builder.Append($"{_indent.Repeat(_indentLevel)}{name}: z.{type}()");
        _openPropertyScope = true;
        return this;
    }
    
    public ZodSchemaBuilder CloseZPropertyScope()
    {
        if(_openPropertyScope == false)
            throw new Exception("Cannot close a property scope while there are no open property scopes.");
        
        _builder.Append(",\n");
        _openPropertyScope = false;
        return this;
    }

    public ZodSchemaBuilder WithValidator(string validator, string parameter = "")
    {
        if(_openPropertyScope == false)
            throw new Exception("Cannot add a validator while there are no open zproperty scopes.");

        if (_validatorAmount == 0)
        {
            _builder.Append($"z.{validator}({parameter})");
        }
        else
        {
            _builder.Append($".{validator}({parameter})");
        }

        _validatorAmount++;
        
        return this;
    }
}