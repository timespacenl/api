using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Permissions.Queries;
using Timespace.Api.Infrastructure.AccessControl;

namespace Timespace.Api.Application.Features.Permissions;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/permissions")]
public class PermissionsController
{
    private readonly ISender _sender;
    private readonly PermissionCollections _permissionCollections;
    
    public PermissionsController(ISender sender, PermissionCollections permissionCollections)
    {
        _sender = sender;
        _permissionCollections = permissionCollections;
    }

    [HttpGet("tenant")]
    public async Task<PermissionTree> GetAvailableTenantScopedPermissions()
    {
        return await _sender.Send(new GetTenantScopedPermissions.Query());
    }
    
    [HttpGet("department")]
    public async Task<PermissionTree> GetAvailableDepartmentScopedPermissions()
    {
        return await _sender.Send(new GetDepartmentScopedPermissions.Query());
    }
}