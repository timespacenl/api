// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    [PermissionGroup("tenant", PermissionScope.Tenant)]
    public partial class Tenant
    {
        public const string Administrator = "tenant:administrator";
        public const string TenantRead = "tenant:read";
        public const string TenantUpdate = "tenant:update";
        public const string TenantDelete = "tenant:delete";
    }
}