using System.Security.Cryptography;

namespace CloudIce3.Shared;

public static class PasswordHasher
{
    // format: iterations.saltBase64.hashBase64
    public static string Hash(string password, int iterations = 100_000, int saltSize = 16, int keySize = 32)
    {
        var salt = RandomNumberGenerator.GetBytes(saltSize);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(keySize);
        return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string stored)
    {
        var parts = stored.Split('.', 3);
        if (parts.Length != 3) return false;

        var iterations = int.Parse(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var key = Convert.FromBase64String(parts[2]);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var candidate = pbkdf2.GetBytes(key.Length);
        return CryptographicOperations.FixedTimeEquals(candidate, key);
    }
}
