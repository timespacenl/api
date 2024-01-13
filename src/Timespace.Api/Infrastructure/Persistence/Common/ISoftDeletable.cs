namespace Timespace.Api.Infrastructure.Persistence.Common;

public interface ISoftDeletable
{
    public Instant? DeletedAt { get; set; }
}