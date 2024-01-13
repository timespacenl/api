namespace Timespace.Api.Application.Features.AccessControl;

public class PermissionGroupAttribute : Attribute
{
    public Type? Parent { get; set; }
    public PermissionScope Scope { get; set; }
}

public enum PermissionScope
{
    Tenant,
    Department
}
