// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    public partial class Tenant
    {
        [PermissionGroup("tenant_settings", "tenant")]
        public class Settings
        {
            public static readonly Permission ReadAll = new Permission("tenant:settings:read_all", PermissionScope.Tenant);
            public static readonly Permission UpdateAll = new Permission("tenant:settings:update_all", PermissionScope.Tenant);

            [PermissionGroup("tenant_settings_apikeys", "tenant_settings")]
            public class ApiKeys
            {
                public static readonly Permission Read = new("tenant:settings:apikeys:read", PermissionScope.Tenant);
                public static readonly Permission Create = new("tenant:settings:apikeys:create", PermissionScope.Tenant);
                public static readonly Permission Update = new("tenant:settings:apikeys:update", PermissionScope.Tenant);
                public static readonly Permission Delete = new("tenant:settings:apikeys:delete", PermissionScope.Tenant);
            }
            
            [PermissionGroup("tenant_settings_roles", "tenant_settings")]
            public class Roles
            {
                public static readonly Permission Read = new("tenant:settings:roles:read", PermissionScope.Tenant);
                public static readonly Permission Create = new("tenant:settings:roles:create", PermissionScope.Tenant);
                public static readonly Permission Update = new("tenant:settings:roles:update", PermissionScope.Tenant);
                public static readonly Permission Delete = new("tenant:settings:roles:delete", PermissionScope.Tenant);            
            }
        }
    }
}

