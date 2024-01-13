using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

    
    
}
