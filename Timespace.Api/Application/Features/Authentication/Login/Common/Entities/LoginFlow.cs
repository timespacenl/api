﻿using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Authentication.Login.Common.Entities;

public class LoginFlow : IEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    public required Instant ExpiresAt { get; set; }
    public required string NextStep { get; set; }
    public bool RememberMe { get; set; }
    
    public Identity Identity { get; set; } = null!;
    public Guid IdentityId { get; set; }
    
    public required List<string> AllowedMethodsForNextStep { get; set; }
}