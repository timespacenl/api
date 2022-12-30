using Microsoft.AspNetCore.Mvc;

namespace Timespace.Api.Application.Features.Tenants.Settings.ApiKeys;

[ApiController]
[Route("v{version:apiVersion}/tenant/settings/apikeys")]
[ApiVersion("1.0")]
public class TenantSettingsApiKeysController : ControllerBase
{
    
}