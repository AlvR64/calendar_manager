using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.Auth.LoginCustomer;

public sealed class LoginCustomerCommandHandler(
    ICustomerRepository customerRepository,
    IPasswordHashingService passwordHashingService,
    IAccessTokenService accessTokenService) : ICommandHandler<LoginCustomerCommand, LoginCustomerResult>
{
    public async Task<LoginCustomerResult> HandleAsync(
        LoginCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        var customer = await customerRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (customer is null || !passwordHashingService.VerifyPassword(command.Password, customer.PasswordHash))
        {
            return LoginCustomerResult.Failure(LoginCustomerError.InvalidCredentials);
        }

        if (!customer.IsActive)
        {
            return LoginCustomerResult.Failure(LoginCustomerError.AccountInactive);
        }

        var accessToken = accessTokenService.CreateForCustomer(customer);

        return LoginCustomerResult.Success(
            accessToken.Token,
            accessToken.TokenType,
            accessToken.ExpiresAtUtc,
            customer.Id,
            customer.Email,
            customer.FirstName,
            customer.LastName);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
