using FluentValidation;
using MediatR;
using Timespace.Api.Application.Features.Authentication.Registration.Common;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Registration.Commands;

public static class CreateRegistrationFlow {
    public record Command(string Email) : IRequest<Response>;

    public record Response(
        Guid FlowId,
        string NextStep,
        Instant ExpiresAt
    ) : IRegistrationFlowResponse;

    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly AppDbContext _db;
        private readonly IClock _clock;
    
        public Handler(AppDbContext db, IClock clock)
        {
            _db = db;
            _clock = clock;
        }
    
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var flow = new RegistrationFlow()
            {
                Email = request.Email,
                NextStep = RegistrationFlowSteps.SetPersonalInformation,
                ExpiresAt = _clock.GetCurrentInstant() + Duration.FromMinutes(5)
            };
            
            _db.RegistrationFlows.Add(flow);
            await _db.SaveChangesAsync(cancellationToken);
            
            return new Response(flow.Id, flow.NextStep, flow.ExpiresAt);
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Email).EmailAddress().NotEmpty();
        }
    }
}