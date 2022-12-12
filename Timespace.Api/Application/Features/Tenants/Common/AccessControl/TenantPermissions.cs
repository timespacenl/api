// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    [PermissionGroup("tenant")]
    public partial class Tenant
    {
        [PermissionMetadata(TenantRead)]
        public const string TenantRead = "tenant:read";
        
    }
}