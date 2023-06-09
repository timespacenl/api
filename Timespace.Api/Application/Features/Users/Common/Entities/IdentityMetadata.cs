namespace Timespace.Api.Application.Features.Users.Common.Entities;

public partial class Identity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public bool RequiresMfa { get; set; }
    public bool AllowLogin { get; set; }
}