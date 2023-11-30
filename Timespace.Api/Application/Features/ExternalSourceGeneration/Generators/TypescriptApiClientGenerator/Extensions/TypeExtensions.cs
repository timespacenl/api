﻿using System.Collections;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Extensions;

public static class TypeExtensions
{
    public static bool IsNullableValueType(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
    
    public static bool IsEnumerableT(this Type type)
    {
        return type.IsGenericType && type.GetInterface(nameof(IEnumerable)) != null;
    }
    
    public static bool IsNodaTimeType(this Type type)
    {
        return type == typeof(Instant) || type == typeof(LocalDate) || type == typeof(LocalDateTime);
    }
    
    public static string GetTsType(this Type type)
    {
        if(Constants.MappableTypesMapping.TryGetValue(type.Name, out var tsType))
        {
            return tsType;
        }
        
        throw new NotImplementedException($"Type {type.Name} is not implemented in TS type mapping");
    }
    
    public static bool IsMappablePrimitive(this Type type)
    {
        if(Constants.MappableTypesMapping.TryGetValue(type.Name, out var tsType))
        {
            return true;
        }
        
        return false;
    }
    
    public static List<GeneratableMember> GetGeneratableMembersFromType(this Type type, string propertyName, bool returnMemberOfMembers = false, HashSet<Type>? seenTypes = null, bool nullable = false, bool list = false)
    {
        if(seenTypes is null)
            seenTypes = new HashSet<Type>();

        var generatableMembers = new List<GeneratableMember>();
        var paramType = type.IsEnumerableT() ? type.GenericTypeArguments.FirstOrDefault() ?? throw new Exception("List argument is null") : type;
            
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
                generatableMember.Members.AddRange(property.PropertyType.GetGeneratableMembersFromType(property.Name, property.Name.ToLower() is "command" or "body", seenTypes, property.IsNullableReferenceType(), property.PropertyType.IsEnumerableT()));
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
                generatableMember.Members.AddRange(property.PropertyType.GetGeneratableMembersFromSharedType(property.Name, sharedTypes, property.Name.ToLower() is "command" or "body", root: false, seenTypes: seenTypes, property.IsNullableReferenceType(), property.PropertyType.IsEnumerableT()));
            }
            
            generatableMembers.Add(generatableMember);
        }
        
        if (returnMemberOfMembers)
        {
            return generatableMember.Members.Count > 0 ? generatableMember.Members : generatableMembers;
        }
        
        return generatableMembers;
    }
    
    public static bool IsSharedType(this List<SharedType> sharedTypes, Type type)
    {
        return sharedTypes.Any(x => x.OriginalType == type);
    }
}
