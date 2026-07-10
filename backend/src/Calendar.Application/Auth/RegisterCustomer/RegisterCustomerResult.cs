namespace Calendar.Application.Auth.RegisterCustomer;

public sealed record RegisterCustomerResult(
    bool Succeeded,
    RegisterCustomerError Error,
    Guid? CustomerId,
    string? Email,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    DateTimeOffset? CreatedAtUtc)
{
    public static RegisterCustomerResult Success(
        Guid customerId,
        string email,
        string firstName,
        string? lastName,
        string? phoneNumber,
        DateTimeOffset createdAtUtc) => new(
            true,
            RegisterCustomerError.None,
            customerId,
            email,
            firstName,
            lastName,
            phoneNumber,
            createdAtUtc);

    public static RegisterCustomerResult Failure(RegisterCustomerError error) => new(
        false,
        error,
        null,
        null,
        null,
        null,
        null,
        null);
}
