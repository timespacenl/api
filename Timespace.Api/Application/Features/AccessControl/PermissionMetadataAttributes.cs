namespace Timespace.Api.Application.Features.AccessControl;

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

public class PermissionScopeAttribute : Attribute
{
    public PermissionScopeAttribute(PermissionScope scope)
    {
        Scope = scope;
    }
    
    public PermissionScope Scope { get; set; }
}