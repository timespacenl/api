using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Timespace.Api.Application.Features.Permissions;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class PermissionsController
{
    private readonly ISender _sender;

    public PermissionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("tenant")]
    public static async Task GetAvailableTenantScopedPermissions()
    {
        
    }
    
    [HttpGet("department")]
    public static async Task GetAvailableDepartmentScopedPermissions()
    {
        
    }
}