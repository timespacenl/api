using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Exceptions;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Registration.Queries;

public static class GetRegistrationFlow {
    public record Query(
        [property:FromRoute(Name = "flowId")] Guid FlowId
    ) : IRequest<Response>;
    
    public record Response(
        Guid FlowId,
        string NextStep
    );
    
    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly AppDbContext _db;
        private readonly IClock _clock;
    
        public Handler(AppDbContext db, IClock clock)
        {
            _db = db;
            _clock = clock;
        }
    
        public Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var flow = _db.RegistrationFlows.FirstOrDefault(x => x.Id == request.FlowId);
            if (flow == null)
                throw new FlowNotFoundException();

            if (flow.ExpiresAt < _clock.GetCurrentInstant())
                throw new FlowExpiredException();
            
            return Task.FromResult(new Response(flow.Id, flow.NextStep));
        }
    }
    
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            
        }
    }
}