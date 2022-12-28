// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    [PermissionGroup("user", PermissionScope.Tenant)]
    public partial class User
    {
        [PermissionGroup("user_settings", PermissionScope.Tenant)]
        public partial class Settings
        {
            
        }
    }
}