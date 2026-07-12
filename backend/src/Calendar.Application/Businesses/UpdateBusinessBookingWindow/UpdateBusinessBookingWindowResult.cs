namespace Calendar.Application.Businesses.UpdateBusinessBookingWindow;

public sealed record UpdateBusinessBookingWindowResult(
    bool Succeeded,
    UpdateBusinessBookingWindowError Error,
    int? MaxAdvanceBookingDays)
{
    public static UpdateBusinessBookingWindowResult Success(int maxAdvanceBookingDays) => new(
        true,
        UpdateBusinessBookingWindowError.None,
        maxAdvanceBookingDays);

    public static UpdateBusinessBookingWindowResult Failure(UpdateBusinessBookingWindowError error) => new(
        false,
        error,
        null);
}
