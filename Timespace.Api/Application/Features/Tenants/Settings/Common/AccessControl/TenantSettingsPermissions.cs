// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    public partial class Tenant
    {
        [PermissionGroup("tenant_settings", PermissionScope.Tenant)]
        public class Settings
        {
            public static readonly Permission ReadAll = new Permission("tenant:settings:read_all");
            public static readonly Permission UpdateAll = new Permission("tenant:settings:update_all");

            [PermissionGroup("tenant_settings_apikeys", PermissionScope.Tenant)]
            public class ApiKeys
            {
                public static readonly Permission Read = new("tenant:settings:apikeys:read");
                public static readonly Permission Create = new("tenant:settings:apikeys:create");
                public static readonly Permission Update = new("tenant:settings:apikeys:update");
                public static readonly Permission Delete = new("tenant:settings:apikeys:delete");
            }
            
            [PermissionGroup("tenant_settings_roles", PermissionScope.Tenant)]
            public class Roles
            {
                public static readonly Permission Read = new("tenant:settings:roles:read");
                public static readonly Permission Create = new("tenant:settings:roles:create");
                public static readonly Permission Update = new("tenant:settings:roles:update");
                public static readonly Permission Delete = new("tenant:settings:roles:delete");            
            }
            
            [PermissionGroup("tenant_settings_employees", PermissionScope.Tenant)]
            public class Employees
            {
                public static readonly Permission Read = new("tenant:settings:employees:read");
                public static readonly Permission Create = new("tenant:settings:employees:create");
                public static readonly Permission Update = new("tenant:settings:employees:update");
                public static readonly Permission Delete = new("tenant:settings:employees:delete");    
                
                // Data such as bsn, etc
                public static readonly Permission ReadExtended = new("tenant:settings:employees:read_extended");
                public static readonly Permission UpdateExtended = new("tenant:settings:employees:update_extended");
            }
        }
    }
}

