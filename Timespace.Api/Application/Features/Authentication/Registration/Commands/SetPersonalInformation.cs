﻿using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Authentication.Registration.Common;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Exceptions;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Registration.Commands;

public static class SetPersonalInformation {
    public record Command(
        [property:FromRoute(Name = "flowId")] Guid FlowId,
        [property:FromBody] CommandBody Body  
    ) : IRequest<Response>;
    
    public record CommandBody(
        string FirstName,
        string LastName,
        string PhoneNumber
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
            
            if(flow.NextStep != RegistrationFlowSteps.SetPersonalInformation)
                throw new InvalidFlowStepException(flow.NextStep);
            
            flow.FirstName = request.Body.FirstName;
            flow.LastName = request.Body.LastName;
            flow.PhoneNumber = request.Body.PhoneNumber;

            flow.NextStep = RegistrationFlowSteps.SetCompanyInformation;
            
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
            RuleFor(x => x.Body.FirstName).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Body.LastName).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Body.PhoneNumber).NotEmpty().MaximumLength(80);
        }
    }
}