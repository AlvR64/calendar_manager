namespace Calendar.Application.Auth.LoginCustomer;

public sealed record LoginCustomerResult(
    bool Succeeded,
    LoginCustomerError Error,
    string? AccessToken,
    string? TokenType,
    DateTimeOffset? ExpiresAtUtc,
    Guid? CustomerId,
    string? Email,
    string? FirstName,
    string? LastName)
{
    public static LoginCustomerResult Success(
        string accessToken,
        string tokenType,
        DateTimeOffset expiresAtUtc,
        Guid customerId,
        string email,
        string firstName,
        string? lastName) => new(
            true,
            LoginCustomerError.None,
            accessToken,
            tokenType,
            expiresAtUtc,
            customerId,
            email,
            firstName,
            lastName);

    public static LoginCustomerResult Failure(LoginCustomerError error) => new(
        false,
        error,
        null,
        null,
        null,
        null,
        null,
        null,
        null);
}
