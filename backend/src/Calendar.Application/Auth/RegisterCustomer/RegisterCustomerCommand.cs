using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Auth.RegisterCustomer;

public sealed record RegisterCustomerCommand(
    string Email,
    string Password,
    string FirstName,
    string? LastName,
    string? PhoneNumber) : ICommand<RegisterCustomerResult>;
