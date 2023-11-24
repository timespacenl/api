using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Timespace.Api.Application.Features.Users.Authentication.Verification;

[ApiController]
[Route("v{version:apiVersion}/auth/verify")]
[ApiVersion("1.0")]
public class VerificationController : ControllerBase
{
    private readonly ISender _sender;

    public VerificationController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpPost("email")]
    public async Task<VerifyEmail.Response> VerifyEmail([FromQuery] VerifyEmail.Command command)
    {
        return await _sender.Send(command);
    }
}