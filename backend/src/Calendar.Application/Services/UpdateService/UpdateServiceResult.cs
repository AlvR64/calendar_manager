namespace Calendar.Application.Services.UpdateService;

public sealed record UpdateServiceResult(
    bool Succeeded,
    UpdateServiceError Error,
    AdminServiceDetails? Service)
{
    public static UpdateServiceResult Success(AdminServiceDetails service) => new(
        true,
        UpdateServiceError.None,
        service);

    public static UpdateServiceResult Failure(UpdateServiceError error) => new(
        false,
        error,
        null);
}
