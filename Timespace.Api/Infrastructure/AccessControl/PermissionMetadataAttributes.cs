namespace Timespace.Api.Infrastructure.AccessControl;

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