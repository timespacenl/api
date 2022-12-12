namespace Timespace.Api.Application.Common.Attributes;

public class PermissionAuthorizeAttribute : Attribute
{
    public PermissionAuthorizeType Type { get; set; }
    public List<string> Permissions { get; set; }
    
    public PermissionAuthorizeAttribute(PermissionAuthorizeType type, params string[] permissions)
    {
        Type = type;
        Permissions = permissions.ToList();
    }
    
    public PermissionAuthorizeAttribute(string permission)
    {
        Type = PermissionAuthorizeType.And;
        Permissions = new List<string> { permission };
    }
}

public enum PermissionAuthorizeType
{
    And,
    Or
}