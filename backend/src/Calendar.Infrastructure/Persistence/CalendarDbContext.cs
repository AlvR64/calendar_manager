using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence;

public sealed class CalendarDbContext(DbContextOptions<CalendarDbContext> options) : DbContext(options)
{
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<StaffMemberService> StaffMemberServices => Set<StaffMemberService>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<StaffMemberAvailability> StaffMemberAvailabilities => Set<StaffMemberAvailability>();
    public DbSet<StaffMemberAvailabilityException> StaffMemberAvailabilityExceptions => Set<StaffMemberAvailabilityException>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CalendarDbContext).Assembly);
    }
}
