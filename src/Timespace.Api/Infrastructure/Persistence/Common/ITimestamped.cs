namespace Timespace.Api.Infrastructure.Persistence.Common;

public interface ITimestamped
{
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
}
