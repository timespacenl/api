// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    public partial class Tenant
    {
        public partial class Settings
        {
            [PermissionGroup("tenant_settings_roles", "tenant_settings")]
            public class Roles
            {
                [PermissionMetadata(Read)]
                public const string Read = "tenant:settings:roles:read";

                [PermissionMetadata(Create)]
                public const string Create = "tenant:settings:roles:create";

                [PermissionMetadata(Update)]
                public const string Update = "tenant:settings:roles:update";

                [PermissionMetadata(Delete)]
                public const string Delete = "tenant:settings:roles:delete";
            }
        }
    }
}