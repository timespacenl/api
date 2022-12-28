using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Infrastructure.AccessControl;

namespace Timespace.Api.Application.Features.Permissions;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
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
    public Task<PermissionTree> GetAvailableTenantScopedPermissions()
    {
        return Task.FromResult(_permissionCollections.AllTenantScoped);
    }
    
    [HttpGet("department")]
    public Task<PermissionTree> GetAvailableDepartmentScopedPermissions()
    {
        return Task.FromResult(_permissionCollections.AllDepartmentScoped);
    }
}