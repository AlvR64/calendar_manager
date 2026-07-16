using System.Data;
using Calendar.Application.Appointments.CreateAppointment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Calendar.Infrastructure.Persistence;

public sealed class AppointmentCreationConcurrencyGuard(CalendarDbContext dbContext) : IAppointmentCreationConcurrencyGuard
{
    private const int LockTimeoutMilliseconds = 5000;

    public async Task<T> ExecuteWithLockAsync<T>(
        string resource,
        Func<CancellationToken, Task<T>> action,
        Func<T> lockNotAcquiredResult,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var lockAcquired = await TryAcquireLockAsync(resource, transaction, cancellationToken);
        if (!lockAcquired)
        {
            await transaction.RollbackAsync(cancellationToken);
            return lockNotAcquiredResult();
        }

        var result = await action(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return result;
    }

    private async Task<bool> TryAcquireLockAsync(
        string resource,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        command.CommandText = "EXEC @result = sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = @lockTimeout;";

        var resultParameter = command.CreateParameter();
        resultParameter.ParameterName = "@result";
        resultParameter.DbType = DbType.Int32;
        resultParameter.Direction = ParameterDirection.Output;
        command.Parameters.Add(resultParameter);

        var resourceParameter = command.CreateParameter();
        resourceParameter.ParameterName = "@resource";
        resourceParameter.DbType = DbType.String;
        resourceParameter.Value = resource;
        command.Parameters.Add(resourceParameter);

        var lockTimeoutParameter = command.CreateParameter();
        lockTimeoutParameter.ParameterName = "@lockTimeout";
        lockTimeoutParameter.DbType = DbType.Int32;
        lockTimeoutParameter.Value = LockTimeoutMilliseconds;
        command.Parameters.Add(lockTimeoutParameter);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return resultParameter.Value is int result && result >= 0;
    }
}
