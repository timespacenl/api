// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    public partial class Tenant
    {
        public partial class Settings
        {
            [PermissionGroup("tenant_settings_apikeys", "tenant_settings")]
            public class ApiKeys
            {
                [PermissionMetadata(Read)]
                public const string Read = "tenant:settings:apikeys:read";

                [PermissionMetadata(Create)]
                public const string Create = "tenant:settings:apikeys:create";

                [PermissionMetadata(Update)]
                public const string Update = "tenant:settings:apikeys:update";

                [PermissionMetadata(Delete)]
                public const string Delete = "tenant:settings:apikeys:delete";
            }
        }
    }
}