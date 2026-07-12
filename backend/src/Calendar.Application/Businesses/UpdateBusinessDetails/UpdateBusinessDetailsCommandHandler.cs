using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.Businesses.UpdateBusinessDetails;

public sealed class UpdateBusinessDetailsCommandHandler(
    IBusinessRepository businessRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateBusinessDetailsCommand, UpdateBusinessDetailsResult>
{
    public async Task<UpdateBusinessDetailsResult> HandleAsync(
        UpdateBusinessDetailsCommand command,
        CancellationToken cancellationToken)
    {
        var business = await businessRepository.GetByIdAsync(command.BusinessId, cancellationToken);
        if (business is null)
        {
            return UpdateBusinessDetailsResult.Failure(UpdateBusinessDetailsError.BusinessNotFound);
        }

        business.Name = command.Name.Trim();
        business.Description = NormalizeOptionalText(command.Description);
        business.ContactEmail = NormalizeOptionalText(command.ContactEmail);
        business.ContactPhoneNumber = NormalizeOptionalText(command.ContactPhoneNumber);
        business.WebsiteUrl = NormalizeOptionalText(command.WebsiteUrl);
        business.AddressLine1 = NormalizeOptionalText(command.AddressLine1);
        business.AddressLine2 = NormalizeOptionalText(command.AddressLine2);
        business.City = NormalizeOptionalText(command.City);
        business.PostalCode = NormalizeOptionalText(command.PostalCode);
        business.CountryCode = NormalizeOptionalText(command.CountryCode)?.ToUpperInvariant();
        business.TimeZoneId = command.TimeZoneId.Trim();
        business.CurrencyCode = command.CurrencyCode.Trim().ToUpperInvariant();
        business.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateBusinessDetailsResult.Success(
            business.Id,
            business.Name,
            business.Slug,
            business.Description,
            business.ContactEmail,
            business.ContactPhoneNumber,
            business.WebsiteUrl,
            business.AddressLine1,
            business.AddressLine2,
            business.City,
            business.PostalCode,
            business.CountryCode,
            business.TimeZoneId,
            business.CurrencyCode,
            business.MaxAdvanceBookingDays);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
