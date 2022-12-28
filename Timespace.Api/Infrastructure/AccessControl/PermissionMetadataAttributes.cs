namespace Timespace.Api.Infrastructure.AccessControl;

public class PermissionMetadataAttribute : Attribute
{
    public PermissionMetadataAttribute(Permission permission)
    {
        PermissionString = permission.Key;
    }

    public string PermissionString { get; set; }
}

public class PermissionGroupAttribute : Attribute
{
    public PermissionGroupAttribute(string groupCode, PermissionScope scope)
    {
        GroupCode = groupCode;
        Scope = scope;
    }
    
    public string GroupCode { get; set; }
    public PermissionScope Scope { get; set; }
}