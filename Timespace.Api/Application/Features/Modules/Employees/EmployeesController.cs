using MediatR;
using Microsoft.AspNetCore.Mvc;
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

    [HttpGet("{employeeId}")]
    public async Task<GetEmployee.Response> GetEmployeeAsync([FromQuery] GetEmployee.Query query)
    {
        return await _sender.Send(query);
    }

    [HttpGet("{employeeId}/extended")]
    public async Task<IActionResult> GetEmployeeExtendedAsync([FromRoute] Guid employeeId)
    {
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployeesAsync()
    {
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> InviteEmployeeAsync([FromBody] string request)
    {
        return Ok();
    }

    [HttpPut("{employeeId}")]
    public async Task<IActionResult> UpdateEmployeeAsync([FromRoute] Guid employeeId, [FromBody] string request)
    {
        return Ok();
    }
}