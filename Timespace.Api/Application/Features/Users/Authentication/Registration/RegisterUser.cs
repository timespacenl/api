using Microsoft.AspNetCore.Identity;
using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.SourceGenerators;

namespace Timespace.Api.Application.Features.Users.Authentication.Registration;

[GenerateMediatr]
public static partial class RegisterUser
{
    public partial record Command();

    public record Response();

    private static async Task<Response> Handle(Command request,
        AppDbContext db,
        ILogger<Handler> logger,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        
        return new Response();
    }

    public partial class Validator
    {
        public Validator()
        {

        }
    }
}
