using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timespace.Api.Application.Common.Attributes;
using Timespace.Api.Application.Features.Users.Authentication.Common.Exceptions;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Users.Authentication.Login.Queries;

public static class GetLoginFlow {
    
    [AllowUnauthenticated]
    public record Query : IRequest<Response>
    {
        [FromRoute(Name = "flowId")]
        public Guid FlowId { get; init; }
    }
    
    public record Response()
    {
        public Guid FlowId { get; init; }
        public Instant ExpiresAt { get; init; }
        public string NextStep { get; init; }
        public List<string> AllowedMethodsForNextStep { get; init; }
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
    
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var flow = _db.LoginFlows.IgnoreQueryFilters().FirstOrDefault(x => x.Id == request.FlowId);
            
            if (flow == null)
                throw new FlowNotFoundException();

            if (flow.ExpiresAt < _clock.GetCurrentInstant())
                throw new FlowExpiredException();

            return new Response
            {
                FlowId = flow.Id,
                ExpiresAt = flow.ExpiresAt,
                NextStep = flow.NextStep,
                AllowedMethodsForNextStep = flow.AllowedMethodsForNextStep
            };
        }
    }
    
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            
        }
    }
}