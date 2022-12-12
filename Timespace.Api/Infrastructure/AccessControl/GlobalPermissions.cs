namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    [PermissionGroup("global")]
    public class GlobalPermissions
    {
        [PermissionMetadata(Administrator)]
        public const string Administrator = "global:administrator";
    }
}
