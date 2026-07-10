using System.Globalization;
using System.Security.Cryptography;
using Calendar.Domain.Abstractions;

namespace Calendar.Infrastructure.Security;

public sealed class Pbkdf2PasswordHashingService : IPasswordHashingService
{
    private const int SaltSizeInBytes = 16;
    private const int HashSizeInBytes = 32;
    private const int IterationCount = 210_000;
    private const char Separator = '$';
    private const string Algorithm = "PBKDF2-SHA256";

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSizeInBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            IterationCount,
            HashAlgorithmName.SHA256,
            HashSizeInBytes);

        return string.Join(
            Separator,
            Algorithm,
            IterationCount.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var parts = passwordHash.Split(Separator);
        if (parts is not [Algorithm, var iterationsValue, var saltValue, var hashValue])
        {
            return false;
        }

        if (!int.TryParse(iterationsValue, CultureInfo.InvariantCulture, out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(saltValue);
            var expectedHash = Convert.FromBase64String(hashValue);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
