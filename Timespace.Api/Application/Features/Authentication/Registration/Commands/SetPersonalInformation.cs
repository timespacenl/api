using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Common.Attributes;
using Timespace.Api.Application.Features.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Authentication.Registration.Common;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Exceptions;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Registration.Commands;

public static class SetPersonalInformation {

    [AllowUnauthenticated]
    public record Command : IRequest<Response>
    {
        [FromRoute(Name = "flowId")] 
        public Guid FlowId { get; init; }
        
        [FromBody] 
        public CommandBody Body { get; init; } = null!;
    }

    public record CommandBody
    {
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string? PhoneNumber { get; init; }
    }

    public class Response : IRegistrationFlowResponse
    {
        public Guid FlowId { get; init; }
        public string NextStep { get; init; } = null!;
        public Instant ExpiresAt { get; init; }
    }

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
            
            if(flow.NextStep != RegistrationFlowSteps.SetPersonalInformation)
                throw new InvalidFlowStepException(flow.NextStep);
            
            flow.FirstName = request.Body.FirstName;
            flow.LastName = request.Body.LastName;
            flow.PhoneNumber = request.Body.PhoneNumber;

            flow.NextStep = RegistrationFlowSteps.SetCompanyInformation;
            
            await _db.SaveChangesAsync(cancellationToken);
            
            return new Response
            {
                FlowId = flow.Id,
                NextStep = flow.NextStep,
                ExpiresAt = flow.ExpiresAt
            };
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.FlowId).NotEmpty();
            RuleFor(x => x.Body).NotNull();
            RuleFor(x => x.Body.FirstName).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Body.LastName).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Body.PhoneNumber).MaximumLength(80);
        }
    }
}