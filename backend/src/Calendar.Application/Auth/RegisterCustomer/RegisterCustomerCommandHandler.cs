using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Auth.RegisterCustomer;

public sealed class RegisterCustomerCommandHandler(
    ICustomerRepository customerRepository,
    IPasswordHashingService passwordHashingService,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterCustomerCommand, RegisterCustomerResult>
{
    public async Task<RegisterCustomerResult> HandleAsync(
        RegisterCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(command.Email);

        if (await customerRepository.ExistsByNormalizedEmailAsync(normalizedEmail, cancellationToken))
        {
            return RegisterCustomerResult.Failure(RegisterCustomerError.CustomerEmailAlreadyExists);
        }

        var now = DateTimeOffset.UtcNow;
        var customerId = Guid.NewGuid();
        var email = command.Email.Trim();
        var firstName = command.FirstName.Trim();
        var lastName = NormalizeOptionalText(command.LastName);
        var phoneNumber = NormalizeOptionalText(command.PhoneNumber);

        var customer = new Customer
        {
            Id = customerId,
            Email = email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = passwordHashingService.HashPassword(command.Password),
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            IsActive = true,
            CreatedAtUtc = now
        };

        customerRepository.Add(customer);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return RegisterCustomerResult.Success(customerId, email, firstName, lastName, phoneNumber, now);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static string? NormalizeOptionalText(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
