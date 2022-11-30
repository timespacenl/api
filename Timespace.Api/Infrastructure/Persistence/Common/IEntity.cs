namespace Timespace.Api.Infrastructure.Persistence.Common;

public interface IEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
}