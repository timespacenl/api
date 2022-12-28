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
    public PermissionGroupAttribute(string groupCode, string? parentGroupCode = null)
    {
        GroupCode = groupCode;
        ParentGroupCode = parentGroupCode;
    }
    
    public string GroupCode { get; set; }
    public string? ParentGroupCode { get; set; }
}