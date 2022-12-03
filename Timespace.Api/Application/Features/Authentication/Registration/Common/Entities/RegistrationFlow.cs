using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;

public class RegistrationFlow : IEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    
    public Instant ExpiresAt { get; set; }
    public required string NextStep { get; set; }
    
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyIndustry { get; set; }
    public int? CompanySize { get; set; }
    public string? CredentialType { get; set; }
}