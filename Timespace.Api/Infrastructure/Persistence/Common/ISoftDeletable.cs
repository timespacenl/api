using NodaTime;

namespace Timespace.Api.Infrastructure.Persistence.Common;

public interface ISoftDeletable
{
    public bool IsDeleted { get; set; }
    public Instant DeletedAt { get; set; }
}