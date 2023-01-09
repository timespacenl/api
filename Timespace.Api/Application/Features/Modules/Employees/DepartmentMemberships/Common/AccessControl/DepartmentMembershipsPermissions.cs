// ReSharper disable once CheckNamespace
namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    public partial class Employees
    {
        public class DepartmentMembershipsPermissions
        {
            public const string Read = "employees:departments:read";
            public const string Add = "employees:departments:add";
            public const string Update = "employees:departments:update";
            public const string Delete = "employees:departments:delete";
        }
    }
}



