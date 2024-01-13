namespace Timespace.Api.Infrastructure.Services;

internal sealed class DateTimeProvider : IClock
{
    public Instant GetCurrentInstant()
    {
        return SystemClock.Instance.GetCurrentInstant();
    }
}
