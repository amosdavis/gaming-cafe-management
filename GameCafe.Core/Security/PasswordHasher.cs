using System.Security.Cryptography;
using System.Text;

namespace GameCafe.Core.Security;

/// <summary>
/// Password hashing service using PBKDF2 for secure storage.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        using (var algorithm = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256))
        {
            var key = Convert.ToBase64String(algorithm.GetBytes(HashSize));
            var salt = Convert.ToBase64String(algorithm.Salt);
            return $"{Iterations}.{salt}.{key}";
        }
    }

    public bool VerifyPassword(string password, string hash)
    {
        var parts = hash.Split('.');
        if (parts.Length != 3)
            return false;

        var iterations = int.Parse(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var key = Convert.FromBase64String(parts[2]);

        using (var algorithm = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
        {
            var keyToCheck = algorithm.GetBytes(HashSize);
            return keyToCheck.SequenceEqual(key);
        }
    }
}
