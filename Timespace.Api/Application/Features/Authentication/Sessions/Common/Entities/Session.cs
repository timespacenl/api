﻿using System.ComponentModel.DataAnnotations.Schema;
using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Authentication.Sessions.Common.Entities;

public class Session : IEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    
    public Identity Identity { get; set; } = null!;
    public Guid IdentityId { get; set; }
    
    public string SessionToken { get; set; } = null!;
    public Instant ExpiresAt { get; set; }
    
    public bool MfaRequired { get; set; }
    public Instant? MfaCompletedAt { get; set; }
    
    
}