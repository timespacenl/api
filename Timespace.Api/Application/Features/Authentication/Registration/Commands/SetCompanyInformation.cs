using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Authentication.Registration.Common;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Exceptions;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Registration.Commands;

public static class SetCompanyInformation {
    public record Command(
        [property:FromRoute(Name = "flowId")] Guid FlowId,
        [property:FromBody] CommandBody Body
        ) : IRequest<Response>;
    
    public record CommandBody(
        string CompanyName,
        string Industry,
        int CompanySize
        );
    
    public record Response(Guid FlowId, string NextStep, Instant ExpiresAt) : IRegistrationFlowResponse;

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

            if(flow.NextStep != RegistrationFlowSteps.SetCompanyInformation)
                throw new InvalidFlowStepException(flow.NextStep);

            flow.CompanyName = request.Body.CompanyName;
            flow.CompanyIndustry = request.Body.Industry;
            flow.CompanySize = request.Body.CompanySize;

            flow.NextStep = RegistrationFlowSteps.SetCredentials;

            await _db.SaveChangesAsync(cancellationToken);
            
            return new Response(flow.Id, flow.NextStep, flow.ExpiresAt);
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.FlowId).NotEmpty();
            RuleFor(x => x.Body).NotNull();
            RuleFor(x => x.Body.CompanyName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Body.Industry).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Body.CompanySize).NotEmpty().LessThanOrEqualTo(1000);
        }
    }
}