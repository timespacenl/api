using Microsoft.AspNetCore.Mvc;

namespace Timespace.Api.Application.Features.Modules.Employees;

[ApiController]
[Route("v{version:apiVersion}/employees")]
[ApiVersion("1.0")]
public class EmployeesController : ControllerBase
{
    
}