using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Users.Settings.Mfa.Commands;

namespace Timespace.Api.Application.Features.Users.Settings.Mfa;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("/v{version:apiVersion}/users/settings/mfa")]
[ApiVersion("1.0")]
public class SettingsMfaController : ControllerBase
{
    private readonly ISender _sender;

    public SettingsMfaController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("setup")]
    public async Task<CreateMfaSetupFlow.Response> SetupMfa()
    {
        return await _sender.Send(new CreateMfaSetupFlow.Command());
    }
    
    [HttpPost("{flowId}/complete")]
    public async Task<CompleteMfaSetupFlow.Response> CompleteMfa([FromBody] CompleteMfaSetupFlow.Command command)
    {
        return await _sender.Send(command);
    }
}