using Timespace.Api.Infrastructure.AccessControl;

namespace Timespace.Api.Application.Features.AccessControl;

public class Permissions
{
    [PermissionScope(PermissionScope.Tenant)]
    public class User
    {
        [PermissionScope(PermissionScope.Tenant)]
        public class Settings
        {
            public const string Read = "tenant:read";
            public const string Update = "tenant:update";
            public const string Delete = "tenant:delete";
        }
    }
    
    [PermissionScope(PermissionScope.Tenant)]
    public class Tenant
    {
        public const string Read = "tenant:read";
        public const string Update = "tenant:update";
        public const string Delete = "tenant:delete";
        
        [PermissionScope(PermissionScope.Tenant)]
        public class Settings
        {
            [PermissionScope(PermissionScope.Tenant)]
            public class Roles
            {
                public const string Read = "tenant:settings:roles:read";
                public const string Create = "tenant:settings:roles:create";
                public const string Update = "tenant:settings:roles:update";
                public const string Delete = "tenant:settings:roles:delete";            
            }
            
            [PermissionScope(PermissionScope.Tenant)]
            public class Employees
            {
                public const string Read = "tenant:settings:employees:read";
                public const string Create = "tenant:settings:employees:create";
                public const string Update = "tenant:settings:employees:update";
                public const string Delete = "tenant:settings:employees:delete";
                
                // Data such as bsn, etc
                public const string ReadConfidential = "tenant:settings:employees:read_confidential";
                public const string UpdateConfidential = "tenant:settings:employees:update_confidential";
            }
            
            [PermissionScope(PermissionScope.Tenant)]
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