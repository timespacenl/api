using FluentValidation;
using MediatR;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Login.Commands;

public static class TestOperation {
    public record Command(string TestName) : IRequest<Response>;
    
    public record Response(string Test);
    
    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly AppDbContext _db;
    
        public Handler(AppDbContext db)
        {
            _db = db;
        }
    
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            return new Response("Test");
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.TestName).NotEmpty().MaximumLength(5).WithMessage("Maximum length is 5");
        }
    }
}