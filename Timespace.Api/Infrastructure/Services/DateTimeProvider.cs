using NodaTime;

namespace Timespace.Api.Infrastructure.Services;

public class DateTimeProvider : IClock
{
    public Instant GetCurrentInstant() => SystemClock.Instance.GetCurrentInstant();
}