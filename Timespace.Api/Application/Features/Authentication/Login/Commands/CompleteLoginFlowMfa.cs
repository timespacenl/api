﻿using FluentValidation;
using MediatR;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Login.Commands;

public static class CompleteLoginFlowMfa {
    public record Command : IRequest<Response>
    {
        
    }
    
    public record Response()
    {
        
    }
    
    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly AppDbContext _db;
    
        public Handler(AppDbContext db)
        {
            _db = db;
        }
    
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            return new Response
            {
            
            };
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            
        }
    }
}