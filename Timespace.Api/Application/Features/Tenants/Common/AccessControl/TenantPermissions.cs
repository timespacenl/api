// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    [PermissionGroup("tenant", PermissionScope.Tenant)]
    public partial class Tenant
    {
        public static readonly Permission Administrator = new ("tenant:administrator");
        public static readonly Permission TenantRead = new("tenant:read");
        public static readonly Permission TenantUpdate = new("tenant:update");
        public static readonly Permission TenantDelete = new("tenant:delete");
    }
}