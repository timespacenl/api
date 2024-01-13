using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Timespace.Api.Application.Features.Users.Authentication.Registration;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/auth/registration")]
public partial class RegistrationController : ControllerBase
{
    private readonly ISender _sender;

    public RegistrationController(ISender sender)
    {
        _sender = sender;
    }
}
