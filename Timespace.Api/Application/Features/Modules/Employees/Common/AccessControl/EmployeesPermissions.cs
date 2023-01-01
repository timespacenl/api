// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    [PermissionGroup("employees", PermissionScope.Tenant)]
    public partial class Employees
    {
        
    }
}

