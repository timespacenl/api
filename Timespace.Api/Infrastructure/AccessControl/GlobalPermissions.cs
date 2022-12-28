namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    [PermissionGroup("global")]
    public class GlobalPermissions
    {
        public static readonly Permission Administrator = new("global:administrator", PermissionScope.Tenant);
    }
}
