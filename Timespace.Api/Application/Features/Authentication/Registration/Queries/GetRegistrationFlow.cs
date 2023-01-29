using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timespace.Api.Application.Common.Attributes;
using Timespace.Api.Application.Features.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Authentication.Registration.Common;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Registration.Queries;

public static class GetRegistrationFlow {
    
    [AllowUnauthenticated]
    public record Query : IRequest<Response>
    {
        [FromQuery(Name = "flow")]
        public Guid FlowId { get; init; }
    }

    public record Response : IRegistrationFlowResponse
    {
        public Guid FlowId { get; init; }
        public string NextStep { get; init; } = null!;
        public Instant ExpiresAt { get; init; }
    } 

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
            var flow = _db.RegistrationFlows.IgnoreQueryFilters().FirstOrDefault(x => x.Id == request.FlowId);
            if (flow == null)
                throw new FlowNotFoundException();

            if (flow.ExpiresAt < _clock.GetCurrentInstant())
                throw new FlowExpiredException();
            
            return Task.FromResult(new Response
            {
                FlowId = flow.Id,
                ExpiresAt = flow.ExpiresAt,
                NextStep = flow.NextStep
            });
        }
    }
    
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
        }
    }
}