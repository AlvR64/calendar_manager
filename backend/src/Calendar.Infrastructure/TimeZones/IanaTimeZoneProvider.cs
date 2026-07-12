using Calendar.Domain.Abstractions;
using TimeZoneConverter;

namespace Calendar.Infrastructure.TimeZones;

public sealed class IanaTimeZoneProvider : ITimeZoneProvider
{
    public bool TryGetIanaTimeZoneInfo(string timeZoneId, out TimeZoneInfo timeZoneInfo)
    {
        timeZoneInfo = default!;

        var normalizedTimeZoneId = timeZoneId.Trim();
        if (normalizedTimeZoneId.Length == 0 || !TZConvert.KnownIanaTimeZoneNames.Contains(normalizedTimeZoneId))
        {
            return false;
        }

        try
        {
            timeZoneInfo = TZConvert.GetTimeZoneInfo(normalizedTimeZoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}
