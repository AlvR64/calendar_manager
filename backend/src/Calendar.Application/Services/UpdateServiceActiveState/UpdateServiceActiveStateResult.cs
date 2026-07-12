namespace Calendar.Application.Services.UpdateServiceActiveState;

public sealed record UpdateServiceActiveStateResult(
    bool Succeeded,
    UpdateServiceActiveStateError Error,
    AdminServiceDetails? Service)
{
    public static UpdateServiceActiveStateResult Success(AdminServiceDetails service) => new(
        true,
        UpdateServiceActiveStateError.None,
        service);

    public static UpdateServiceActiveStateResult Failure(UpdateServiceActiveStateError error) => new(
        false,
        error,
        null);
}
