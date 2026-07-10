using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Auth.LoginCustomer;

public sealed record LoginCustomerCommand(
    string Email,
    string Password) : ICommand<LoginCustomerResult>;
