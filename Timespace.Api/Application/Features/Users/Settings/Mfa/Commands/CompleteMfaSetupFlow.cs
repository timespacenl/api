using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Users.Settings.Mfa.Commands;

public static class CompleteMfaSetupFlow {
    public record Command : IRequest<Response>
    {
        [FromRoute(Name = "flowId")]
        public Guid FlowId { get; init; }
        
        [FromBody]
        public CommandBody Body { get; init; } = null!;
    }

    public record CommandBody
    {
        public int TotpCode { get; init; }
    }
    
    public record Response()
    {
        bool Success { get; init; }
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
            RuleFor(x => x.FlowId).NotEmpty();
            RuleFor(x => x.Body).NotNull();
            RuleFor(x => x.Body.TotpCode).NotEmpty();
        }
    }
}