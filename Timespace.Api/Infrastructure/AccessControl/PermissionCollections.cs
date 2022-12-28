namespace Timespace.Api.Infrastructure.AccessControl;

public class PermissionCollections
{
    public PermissionTree All { get; set; }
    public PermissionTree AllTenantScoped { get; set; }
    public PermissionTree AllDepartmentScoped { get; set; }
}