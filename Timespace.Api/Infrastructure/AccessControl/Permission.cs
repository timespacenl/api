namespace Timespace.Api.Infrastructure.AccessControl;

public record Permission(string Key, PermissionScope Scope);

public enum PermissionScope
{
    Tenant,
    Department
}