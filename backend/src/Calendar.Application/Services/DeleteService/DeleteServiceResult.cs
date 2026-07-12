namespace Calendar.Application.Services.DeleteService;

public sealed record DeleteServiceResult(bool Succeeded, DeleteServiceError Error)
{
    public static DeleteServiceResult Success() => new(true, DeleteServiceError.None);

    public static DeleteServiceResult Failure(DeleteServiceError error) => new(false, error);
}
