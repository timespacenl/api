using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Users.Settings.Mfa.Commands;

namespace Timespace.Api.Application.Features.Users.Settings.Mfa;

[ApiController]
[Route("/v{version:apiVersion}/users/settings/mfa")]
[ApiVersion("1.0")]
[Tags("User MFA settings")]
public class UserSettingsMfaController : ControllerBase
{
    private readonly ISender _sender;

    public UserSettingsMfaController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("setup")]
    public async Task<CreateMfaSetupFlow.Response> SetupMfa(CreateMfaSetupFlow.Command command)
    {
        return await _sender.Send(command);
    }
    
    [HttpPost("{flowId}/complete")]
    public async Task<CompleteMfaSetupFlow.Response> CompleteMfa([FromQuery] CompleteMfaSetupFlow.Command command)
    {
        return await _sender.Send(command);
    }
    
    [HttpPost("recovery-codes")]
    public async Task<GenerateMfaRecoveryCodes.Response> GenerateRecoveryCodes()
    {
        return await _sender.Send(new GenerateMfaRecoveryCodes.Command());
    }
}