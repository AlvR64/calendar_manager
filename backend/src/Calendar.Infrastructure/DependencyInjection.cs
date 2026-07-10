using Calendar.Domain.Abstractions;
using Calendar.Infrastructure.Persistence;
using Calendar.Infrastructure.Persistence.Repositories;
using Calendar.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Calendar.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<CalendarDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IBusinessRepository, BusinessRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHashingService, Pbkdf2PasswordHashingService>();

        return services;
    }
}
