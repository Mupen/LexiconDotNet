using System.Security.Cryptography;

namespace DataDrivenCaching.Infrastructure.Security;

// WHAT:
// PasswordHashing creates and verifies PBKDF2 password hashes.
//
// WHY:
// The login demo needs accounts, but storing raw passwords would teach the
// wrong lesson. This concrete helper makes the security rule visible: the
// database stores a salted hash, while the raw password only exists briefly in
// request memory during hashing or verification.
//
// DATA DESIGN:
// The returned string is still sensitive backend data, but it is not the raw
// password. It can be stored in SQLite. It should never be written to browser
// storage, sent back to JavaScript, or used as a frontend authority signal.
public static class PasswordHashing
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"PBKDF2-SHA256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('$');

        if (parts.Length != 4 || parts[0] != "PBKDF2-SHA256")
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expectedHash = Convert.FromBase64String(parts[3]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
