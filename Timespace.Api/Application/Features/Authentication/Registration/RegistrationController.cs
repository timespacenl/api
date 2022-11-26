using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Authentication.Registration.Commands;
using Timespace.Api.Application.Features.Authentication.Registration.Queries;

namespace Timespace.Api.Application.Features.Authentication.Registration;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class RegistrationController : Controller
{
    private readonly ISender _sender;

    public RegistrationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<CreateRegistrationFlow.Response> CreateRegistrationFlow(CreateRegistrationFlow.Command command)
    {
        return await _sender.Send(command);
    }
    
    [HttpGet("{flowId}")]
    [MapToApiVersion("1.0")]
    public async Task<GetRegistrationFlow.Response> GetRegistrationFlow(GetRegistrationFlow.Query query)
    {
        return await _sender.Send(query);
    }
}