using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Auth.RegisterBusiness;
using Microsoft.Extensions.DependencyInjection;

namespace Calendar.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<RegisterBusinessCommand, RegisterBusinessResult>, RegisterBusinessCommandHandler>();

        return services;
    }
}
