using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Auth.RegisterBusiness;
using Calendar.Application.Auth.RegisterCustomer;
using Microsoft.Extensions.DependencyInjection;

namespace Calendar.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<RegisterBusinessCommand, RegisterBusinessResult>, RegisterBusinessCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterCustomerCommand, RegisterCustomerResult>, RegisterCustomerCommandHandler>();

        return services;
    }
}
