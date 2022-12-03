﻿using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Timespace.Api.Application.Features.Authentication.Registration.Common;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Exceptions;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Registration.Commands;

public static class CreateRegistrationFlow {
    public record Command : IRequest<Response>
    {
        public string Email { get; init; } = null!;
    }

    public record Response : IRegistrationFlowResponse
    {
        public Guid FlowId { get; set; }
        public string NextStep { get; set; } = null!;
        public Instant ExpiresAt { get; set; }
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
            var existingCredential = await _db.IdentityIdentifiers.FirstOrDefaultAsync(x => x.Identifier == request.Email, cancellationToken: cancellationToken);
            var existingFlow = await _db.RegistrationFlows.FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken: cancellationToken);
            
            if (existingCredential != null || existingFlow != null)
                throw new DuplicateIdentifierException();
            
            
            var flow = new RegistrationFlow()
            {
                Email = request.Email.ToLower(),
                NextStep = RegistrationFlowSteps.SetPersonalInformation,
                ExpiresAt = _clock.GetCurrentInstant() + Duration.FromMinutes(5)
            };
            
            _db.RegistrationFlows.Add(flow);
            await _db.SaveChangesAsync(cancellationToken);
            
            return new Response()
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
            RuleFor(x => x.Email).EmailAddress().NotEmpty();
        }
    }
}