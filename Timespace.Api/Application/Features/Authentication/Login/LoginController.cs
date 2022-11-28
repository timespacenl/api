using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Authentication.Login.Commands;

namespace Timespace.Api.Application.Features.Authentication.Login;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/login")]
public class LoginController : Controller
{
    private readonly ISender _sender;

    public LoginController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<TestOperation.Response> Index(TestOperation.Command command)
    {
        return await _sender.Send(command);
    }
}