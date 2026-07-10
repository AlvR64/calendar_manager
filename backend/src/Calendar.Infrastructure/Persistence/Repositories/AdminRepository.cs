using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class AdminRepository(CalendarDbContext dbContext) : IAdminRepository
{
    public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        dbContext.Admins.AnyAsync(admin => admin.NormalizedEmail == normalizedEmail, cancellationToken);

    public void Add(Admin admin) => dbContext.Admins.Add(admin);
}
