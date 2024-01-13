using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.SourceGenerators;

namespace Timespace.Api.Application.Features.Users.Authentication.Registration;

public partial class RegistrationController
{
    [HttpPost]
    public async Task<RegisterUser.Response> RegisterUser([FromBody] RegisterUser.Command command)
    {
        return await _sender.Send(command);
    }
}

[GenerateMediatr]
public static partial class RegisterUser
{
    public partial record Command(
        string Email,
        string Password
        );

    public record Response(bool Success);

    private static async Task<Response> Handle(Command request,
        AppDbContext db,
        ILogger<Handler> logger,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        await userManager.CreateAsync(new ApplicationUser()
        {
            Email = request.Email,
            UserName = request.Email
        }, request.Password);
        return new Response(true);
    }

    public partial class Validator
    {
        public Validator()
        {

        }
    }
}
