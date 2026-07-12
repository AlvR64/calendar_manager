using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Auth.RegisterBusiness;

public sealed class RegisterBusinessCommandHandler(
    IBusinessRepository businessRepository,
    IAdminRepository adminRepository,
    IPasswordHashingService passwordHashingService,
    ITimeZoneProvider timeZoneProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterBusinessCommand, RegisterBusinessResult>
{
    public async Task<RegisterBusinessResult> HandleAsync(
        RegisterBusinessCommand command,
        CancellationToken cancellationToken)
    {
        var businessSlug = NormalizeSlug(command.BusinessSlug);
        var normalizedAdminEmail = NormalizeEmail(command.AdminEmail);
        var timeZoneId = command.TimeZoneId.Trim();

        if (!timeZoneProvider.TryGetIanaTimeZoneInfo(timeZoneId, out _))
        {
            return RegisterBusinessResult.Failure(RegisterBusinessError.InvalidTimeZoneId);
        }

        if (await businessRepository.ExistsBySlugAsync(businessSlug, cancellationToken))
        {
            return RegisterBusinessResult.Failure(RegisterBusinessError.BusinessSlugAlreadyExists);
        }

        if (await adminRepository.ExistsByNormalizedEmailAsync(normalizedAdminEmail, cancellationToken))
        {
            return RegisterBusinessResult.Failure(RegisterBusinessError.AdminEmailAlreadyExists);
        }

        var now = DateTimeOffset.UtcNow;
        var businessId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var adminEmail = command.AdminEmail.Trim();

        var business = new Business
        {
            Id = businessId,
            Name = command.BusinessName.Trim(),
            Slug = businessSlug,
            TimeZoneId = timeZoneId,
            CurrencyCode = command.CurrencyCode.Trim().ToUpperInvariant(),
            IsActive = true,
            CreatedAtUtc = now
        };

        var admin = new Admin
        {
            Id = adminId,
            BusinessId = businessId,
            Email = adminEmail,
            NormalizedEmail = normalizedAdminEmail,
            PasswordHash = passwordHashingService.HashPassword(command.AdminPassword),
            DisplayName = command.AdminDisplayName.Trim(),
            IsActive = true,
            CreatedAtUtc = now
        };

        businessRepository.Add(business);
        adminRepository.Add(admin);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return RegisterBusinessResult.Success(businessId, businessSlug, adminId, adminEmail, now);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static string NormalizeSlug(string slug) => slug.Trim().ToLowerInvariant();
}
