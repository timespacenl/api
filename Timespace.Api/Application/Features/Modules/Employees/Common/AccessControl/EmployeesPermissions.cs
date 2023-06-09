// ReSharper disable once CheckNamespace

using Timespace.Api.Application.Features.AccessControl;

namespace Timespace.Api.Infrastructure.AccessControl;

public partial class Permissions
{
    [PermissionGroup("employees", PermissionScope.Tenant)]
    public partial class Employees
    {
        public const string Read = "employees:read";
        public const string Invite = "employees:invite";
        public const string Update = "employees:update";
        public const string Delete = "employees:delete";
    }
}

