using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Timespace.Api.Application.Features.Tenants.Settings.Departments;

[ApiController]
[Route("v{version:apiVersion}/tenant/settings/departments")]
[ApiVersion("1.0")]
public class TenantSettingsDepartmentsController : ControllerBase
{
    
}