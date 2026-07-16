using Calendar.Domain.Abstractions;
using Calendar.Application.Appointments.CreateAppointment;
using Calendar.Infrastructure.Persistence;
using Calendar.Infrastructure.Persistence.Repositories;
using Calendar.Infrastructure.Security;
using Calendar.Infrastructure.TimeZones;
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

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
            .Validate(options => options.SigningKey.Length >= 32, "JWT signing key must be at least 32 characters long.")
            .Validate(options => options.AccessTokenMinutes > 0, "JWT access token lifetime must be positive.")
            .ValidateOnStart();

        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IBusinessRepository, BusinessRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IStaffMemberRepository, StaffMemberRepository>();
        services.AddScoped<IStaffMemberAvailabilityRepository, StaffMemberAvailabilityRepository>();
        services.AddScoped<IStaffMemberAvailabilityExceptionRepository, StaffMemberAvailabilityExceptionRepository>();
        services.AddScoped<IStaffMemberServiceRepository, StaffMemberServiceRepository>();
        services.AddScoped<IAppointmentCreationConcurrencyGuard, AppointmentCreationConcurrencyGuard>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IAccessTokenService, JwtAccessTokenService>();
        services.AddSingleton<IPasswordHashingService, Pbkdf2PasswordHashingService>();
        services.AddSingleton<ITimeZoneProvider, IanaTimeZoneProvider>();

        return services;
    }
}
