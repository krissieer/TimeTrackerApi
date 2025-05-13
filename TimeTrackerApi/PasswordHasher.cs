using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace TimeTrackerApi;

public class PasswordHasher
{
    public static string HashPassword(string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }
        catch (ArgumentException ex)
        {
            // Логирование ошибок ввода
            Console.WriteLine($"Input Error: {ex.Message}");
            throw;
        }
        catch (CryptographicException ex)
        {
            // Логирование ошибок криптографических операций
            Console.WriteLine($"Cryptography Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            // Логирование других ошибок
            Console.WriteLine($"Unexpected Error: {ex.Message}");
            throw;
        }
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split(':');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var storedHash = parts[1];

        string hashToVerify = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 32));

        return hashToVerify == storedHash;
    }
}
