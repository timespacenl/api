using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Modules.Employees.Common;

namespace Timespace.Api.Application.Features.Modules.Employees;

[ApiController]
[Route("v{version:apiVersion}/employees")]
[ApiVersion("1.0")]
public class EmployeesController : ControllerBase
{
    private readonly ISender _sender;

    public EmployeesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{employeeId}")]
    public async Task<PaginatedResult<UserDto>?> GetEmployeeAsync(int employeeId, [FromBody] SharedType query)
    {
        return null;
    }
    
    [HttpGet("{employeeId}/2")]
    public async Task<PaginatedResult<UserDto>> GetEmployee2Async(int employeeId, [FromBody] UserDto body)
    {
        return null;
    }
    
    // [HttpGet("{employeeId}/3")]
    // public async Task<PaginatedResult<UserDto>> GetEmployee3Async([FromBody] UserDto body)
    // {
    //     return null;
    // }

    public record UserDto
    {
        public string Name { get; init; } = null!;
        public string Email { get; init; } = null!;
    }
    
    // [HttpGet("{employeeId}/extended")]
    // public async Task<IActionResult> GetEmployeeExtendedAsync([FromRoute] Guid employeeId)
    // {
    //     return Ok();
    // }
    //
    // [HttpGet]
    // public async Task<IActionResult> GetEmployeesAsync()
    // {
    //     return Ok();
    // }

    // [HttpPost]
    // public async Task<IActionResult> InviteEmployeeAsync([FromBody] string request)
    // {
    //     return Ok();
    // }
    //
    // [HttpPut("{employeeId}")]
    // public async Task<IActionResult> UpdateEmployeeAsync([FromRoute] Guid employeeId, [FromBody] string request)
    // {
    //     return Ok();
    // }
}
