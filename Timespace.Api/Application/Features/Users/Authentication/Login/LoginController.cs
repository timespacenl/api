using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Users.Authentication.Login.Commands;
using Timespace.Api.Application.Features.Users.Authentication.Login.Queries;

namespace Timespace.Api.Application.Features.Users.Authentication.Login;

[ApiController]
[ApiVersion("2.0")]
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
    public async Task<GetLoginFlow.Response> Get([FromQuery] GetLoginFlow.Query query)
    {
        return await _sender.Send(query);
    }

    [HttpPost]
    public async Task<CreateLoginFlow.Response> CreateLoginFlow(CreateLoginFlow.Command command)
    {
        return await _sender.Send(command);
    }
    
    [HttpPost("{flowId}/credentials")]
    public async Task<SetLoginFlowCredentials.Response> CompleteLoginFlow([FromQuery] SetLoginFlowCredentials.Command command)
    {
        return await _sender.Send(command);
    }
    
    [HttpPost("{flowId}/mfa")]
    public async Task<CompleteLoginFlowMfa.Response> SetLoginFlowMfa([FromQuery] CompleteLoginFlowMfa.Command command)
    {
        return await _sender.Send(command);
    }
    
}