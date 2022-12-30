// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    public partial class Tenant
    {
        [PermissionGroup("tenant_settings", PermissionScope.Tenant)]
        public class Settings
        {
            public const string ReadAll = "tenant:settings:read_all";
            public const string UpdateAll = "tenant:settings:update_all";

            [PermissionGroup("tenant_settings_apikeys", PermissionScope.Tenant)]
            public class ApiKeys
            {
                public const string Read = "tenant:settings:apikeys:read";
                public const string Create = "tenant:settings:apikeys:create";
                public const string Update = "tenant:settings:apikeys:update";
                public const string Delete = "tenant:settings:apikeys:delete";
            }
            
            [PermissionGroup("tenant_settings_roles", PermissionScope.Tenant)]
            public class Roles
            {
                public const string Read = "tenant:settings:roles:read";
                public const string Create = "tenant:settings:roles:create";
                public const string Update = "tenant:settings:roles:update";
                public const string Delete = "tenant:settings:roles:delete";            
            }
            
            [PermissionGroup("tenant_settings_employees", PermissionScope.Tenant)]
            public class Employees
            {
                public const string Read = "tenant:settings:employees:read";
                public const string Create = "tenant:settings:employees:create";
                public const string Update = "tenant:settings:employees:update";
                public const string Delete = "tenant:settings:employees:delete";
                
                // Data such as bsn, etc
                public const string ReadExtended = "tenant:settings:employees:read_extended";
                public const string UpdateExtended = "tenant:settings:employees:update_extended";
            }
            
            [PermissionGroup("tenant_settings_departments", PermissionScope.Tenant)]
            public class Departments
            {
                public const string Read = "tenant:settings:departments:read";
                public const string Create = "tenant:settings:departments:create";
                public const string Update = "tenant:settings:departments:update";
                public const string Delete = "tenant:settings:departments:delete";            
            }

        }
    }
}

