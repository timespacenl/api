using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Users.Authentication.Registration.Commands;
using Timespace.Api.Application.Features.Users.Authentication.Registration.Queries;

namespace Timespace.Api.Application.Features.Users.Authentication.Registration;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/auth/registration")]
public class RegistrationController : ControllerBase
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
    
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<GetRegistrationFlow.Response> GetRegistrationFlow([FromQuery] GetRegistrationFlow.Query query)
    {
        return await _sender.Send(query);
    }
    
    [HttpPost("{flowId}/personal_information")]
    [MapToApiVersion("1.0")]
    public async Task<SetPersonalInformation.Response> SetPersonalInformation([FromQuery] SetPersonalInformation.Command command)
    {
        return await _sender.Send(command);
    }
    
    [HttpPost("{flowId}/company_information")]
    [MapToApiVersion("1.0")]
    public async Task<SetCompanyInformation.Response> SetCompanyInformation([FromQuery] SetCompanyInformation.Command command)
    {
        return await _sender.Send(command);
    }
    
    [HttpPost("{flowId}/credentials")]
    [MapToApiVersion("1.0")]
    public async Task<CompleteRegistrationFlow.Response> CompleteRegistrationFlow([FromQuery] CompleteRegistrationFlow.Command command)
    {
        return await _sender.Send(command);
    }
}