namespace Timespace.Api.Infrastructure.Persistence.Common;

public interface IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}