namespace Calendar.Domain.Abstractions;

public interface ITimeZoneProvider
{
    bool TryGetIanaTimeZoneInfo(string timeZoneId, out TimeZoneInfo timeZoneInfo);
}
