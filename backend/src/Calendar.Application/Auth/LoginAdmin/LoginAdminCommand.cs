using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Auth.LoginAdmin;

public sealed record LoginAdminCommand(
    string Email,
    string Password) : ICommand<LoginAdminResult>;
