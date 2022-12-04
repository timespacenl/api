using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Authentication.Login.Commands;
using Timespace.Api.Application.Features.Authentication.Login.Common.Entities;

namespace Timespace.Api.Application.Features.Authentication.Login;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/auth/login")]
public class LoginController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IClock _clock;
    
    public LoginController(ISender sender, IClock clock)
    {
        _sender = sender;
        _clock = clock;
    }

    [HttpGet("{flowId}")]
    public async Task<IActionResult> Get(string flowId)
    {
        var cookies = Request.Cookies;
        // var result = await _sender.Send(new GetLoginFlowQuery(flowId));
        return Ok();
    }

    [HttpPost]
    public async Task<CreateLoginFlow.Response> CreateLoginFlow(CreateLoginFlow.Command command)
    {
        return await _sender.Send(command);
    }
    
    [HttpPost("{flowId}/credentials")]
    public async Task<SetLoginFlowCredentials.Response> CompleteLoginFlow([FromQuery] SetLoginFlowCredentials.Command command)
    {
        var result = await _sender.Send(command);

        if (result.SessionToken != null)
        {
            Response.Cookies.Append("session", result.SessionToken, new CookieOptions
            {
                Expires = _clock.GetCurrentInstant().Plus(Duration.FromDays(30)).ToDateTimeOffset(),
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            });
        }
        
        return result;
    }
    
    [HttpPost("{flowId}/mfa")]
    public async Task<CompleteLoginFlowMfa.Response> SetLoginFlowMfa([FromQuery] CompleteLoginFlowMfa.Command command)
    {
        return await _sender.Send(command);
    }
    
}