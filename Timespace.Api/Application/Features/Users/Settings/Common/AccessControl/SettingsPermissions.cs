// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    public partial class Tenant
    {
        [PermissionGroup("tenant_settings", "tenant")]
        public partial class Settings
        {
            
        }
    }
}