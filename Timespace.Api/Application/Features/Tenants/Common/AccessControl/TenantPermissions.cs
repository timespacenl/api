// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    [PermissionGroup("tenant", PermissionScope.Tenant)]
    public partial class Tenant
    {
        public const string Administrator = "tenant:administrator";
        public const string Read = "tenant:read";
        public const string Update = "tenant:update";
        public const string Delete = "tenant:delete";
    }
}