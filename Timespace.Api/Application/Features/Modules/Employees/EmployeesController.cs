using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Common.Attributes;
using Timespace.Api.Application.Features.Modules.Employees.Common;
using Timespace.Api.Application.Features.Modules.Employees.Queries;

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

    [HttpGet("{employeeId:int}")]
    public async Task<PaginatedResult<UserDto>?> GetEmployeeAsync(int employeeId, [FromBody] SharedType body, string test)
    {
        return null;
    }
    
    [HttpGet("{employeeId:int}/2")]
    public async Task<PaginatedResult<UserDto>> GetEmployee2Async(int employeeId, [FromBody] UserDto body)
    {
        return null;
    }
    
    [HttpGet("{employeeId:int}/3")]
    public async Task<PaginatedResult<UserDto>> GetEmployee3Async(int employeeId, [FromBody] PaginatedResult<UserDto> body)
    {
        return null;
    }
    
    [HttpGet("")]
    public async Task<PaginatedResult<UserDto>> GetEmployee4Async([FromQuery] CreateEmployee.Command command, [FromBody] CreateEmployee.CommandBody body)
    {
        return null;
    }
    
    [HttpGet("get")]
    public async Task<PaginatedResult<UserDto>> GetEmployee5Async([FromQuery] CreateEmployee.Command command, [FromForm] CreateEmployee.CommandBody body, [FromForm] string Test)
    {
        return null;
    }
    
    [HttpGet("get2")]
    public async Task<PaginatedResult<UserDto>> GetEmployee6Async(CreateEmployee.Command2 command)
    {
        return null;
    }
    
    [HttpGet("get3")]
    public async Task<PaginatedResult<UserDto>> GetEmployee7Async([FromQuery] CreateEmployee.Command3 command, SharedType sharedType)
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
