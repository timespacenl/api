namespace Timespace.Api.Infrastructure.AccessControl;

public record Permission(string Key);

public enum PermissionScope
{
    Tenant,
    Department
}