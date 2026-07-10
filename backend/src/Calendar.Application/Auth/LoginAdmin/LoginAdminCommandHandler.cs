using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.Auth.LoginAdmin;

public sealed class LoginAdminCommandHandler(
    IAdminRepository adminRepository,
    IPasswordHashingService passwordHashingService,
    IAccessTokenService accessTokenService) : ICommandHandler<LoginAdminCommand, LoginAdminResult>
{
    public async Task<LoginAdminResult> HandleAsync(
        LoginAdminCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        var admin = await adminRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (admin is null || !passwordHashingService.VerifyPassword(command.Password, admin.PasswordHash))
        {
            return LoginAdminResult.Failure(LoginAdminError.InvalidCredentials);
        }

        if (!admin.IsActive)
        {
            return LoginAdminResult.Failure(LoginAdminError.AccountInactive);
        }

        var accessToken = accessTokenService.CreateForAdmin(admin);

        return LoginAdminResult.Success(
            accessToken.Token,
            accessToken.TokenType,
            accessToken.ExpiresAtUtc,
            admin.Id,
            admin.BusinessId,
            admin.Email,
            admin.DisplayName);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
