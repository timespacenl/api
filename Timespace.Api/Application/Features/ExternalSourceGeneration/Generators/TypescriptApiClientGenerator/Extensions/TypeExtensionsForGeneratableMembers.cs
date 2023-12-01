using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Extensions;

public static class TypeExtensionsForGeneratableMembers
{
    public static List<GeneratableMember> GetGeneratableMembersFromType(this Type type, string propertyName, bool returnMemberOfMembers = false, HashSet<Type>? seenTypes = null, bool nullable = false, bool list = false)
    {
        if(seenTypes is null)
            seenTypes = new HashSet<Type>();

        var generatableMembers = new List<GeneratableMember>();
        var paramType = type.IsEnumerableT() ? type.GenericTypeArguments.FirstOrDefault() ?? throw new Exception("List argument is null") : type;
        paramType = type.IsNullableValueType() ? type.GenericTypeArguments.FirstOrDefault() ?? throw new Exception("Nullable argument is null") : paramType;    
        
        var generatableMember = new GeneratableMember()
        {
            Name = propertyName,
            MemberType = paramType,
            IsNullable = nullable,
            IsList = list
        };
        
        if (paramType.IsMappablePrimitive() || seenTypes.Contains(paramType))
        {
            generatableMembers.Add(generatableMember);
            seenTypes.Add(paramType);
        }
        else
        {
            foreach (var property in paramType.GetProperties())
            {
                seenTypes.Add(paramType);   
                generatableMember.Members.AddRange(property.PropertyType.GetGeneratableMembersFromType(property.Name, property.Name.ToLower() is "command" or "body", seenTypes, property.IsNullable() || property.PropertyType.IsNullableValueType(), property.PropertyType.IsEnumerableT()));
            }
            
            generatableMembers.Add(generatableMember);
        }
        
        if (returnMemberOfMembers)
        {
            return generatableMember.Members;
        }
        
        return generatableMembers;
    }
    
    public static List<GeneratableMember> GetGeneratableMembersFromSharedType(this Type type, string propertyName, List<Type> sharedTypes, bool returnMemberOfMembers = false, bool root = true, HashSet<Type>? seenTypes = null, bool nullable = false, bool list = false)
    {
        if(seenTypes is null)
            seenTypes = new HashSet<Type>();

        var generatableMembers = new List<GeneratableMember>();
        var paramType = type.IsEnumerableT() ? type.GenericTypeArguments.FirstOrDefault() ?? throw new Exception("List argument is null") : type;
        paramType = type.IsNullableValueType() ? type.GenericTypeArguments.FirstOrDefault() ?? throw new Exception("Nullable argument is null") : paramType;    
        
        var generatableMember = new GeneratableMember()
        {
            Name = propertyName,
            MemberType = paramType,
            IsNullable = nullable,
            IsList = list
        };
        
        if (paramType.IsMappablePrimitive() || (sharedTypes.Contains(type) && !root) || seenTypes.Contains(paramType))
        {
            seenTypes.Add(paramType);   
            generatableMembers.Add(generatableMember);
        }
        else
        {
            foreach (var property in paramType.GetProperties())
            {
                seenTypes.Add(paramType);
                generatableMember.Members.AddRange(property.PropertyType.GetGeneratableMembersFromSharedType(property.Name, sharedTypes, property.Name.ToLower() is "command" or "body", root: false, seenTypes: seenTypes, property.IsNullable(), property.PropertyType.IsEnumerableT()));
            }
            
            generatableMembers.Add(generatableMember);
        }
        
        if (returnMemberOfMembers)
        {
            return generatableMember.Members.Count > 0 ? generatableMember.Members : generatableMembers;
        }
        
        return generatableMembers;
    }
}
