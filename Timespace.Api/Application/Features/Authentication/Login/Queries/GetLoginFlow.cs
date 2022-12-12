﻿using FluentValidation;
using MediatR;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Login.Queries;

public static class GetLoginFlow {
    public record Query : IRequest<Response>
    {
        
    }
    
    public record Response()
    {
        
    }
    
    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly AppDbContext _db;
    
        public Handler(AppDbContext db)
        {
            _db = db;
        }
    
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            return new Response
            {
            
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