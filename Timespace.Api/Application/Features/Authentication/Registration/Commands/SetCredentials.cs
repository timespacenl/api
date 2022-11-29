using Destructurama.Attributed;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Exceptions;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Registration.Commands;

public static class SetCredentials {
    public record Command : IRequest<Response>
    {
        [FromRoute(Name = "flowId")] 
        public Guid FlowId { get; init; }
        
        [FromBody] 
        public CommandBody Body { get; init; } = null!;
    }

    public record CommandBody
    {
        [LogMasked] 
        public string? Password { get; init; }
        public bool AcceptTerms { get; init; }
        public bool MagicLink { get; init; }
    }

    public record Response();
    
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
            var flow = _db.RegistrationFlows.FirstOrDefault(x => x.Id == request.FlowId);
            if (flow == null)
                throw new FlowNotFoundException();

            if (flow.ExpiresAt < _clock.GetCurrentInstant())
                throw new FlowExpiredException();

            if(flow.NextStep != RegistrationFlowSteps.SetCredentials)
                throw new InvalidFlowStepException(flow.NextStep);
            
            flow.NextStep = RegistrationFlowSteps.None;

            await _db.SaveChangesAsync(cancellationToken);
            
            return new Response();
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.FlowId).NotEmpty();
            RuleFor(x => x.Body).NotNull();
            RuleFor(x => x.Body.Password).MaximumLength(100);
            RuleFor(x => x.Body.AcceptTerms).Equal(true);
            RuleFor(x => x.Body.MagicLink).NotNull();
        }
    }
}