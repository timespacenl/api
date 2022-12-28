// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    [PermissionGroup("tenant")]
    public partial class Tenant
    {
        public static readonly Permission TenantRead = new("tenant:read", PermissionScope.Tenant);
        public static readonly Permission TenantUpdate = new("tenant:update", PermissionScope.Tenant);
        public static readonly Permission TenantDelete = new("tenant:delete", PermissionScope.Tenant);
    }
}