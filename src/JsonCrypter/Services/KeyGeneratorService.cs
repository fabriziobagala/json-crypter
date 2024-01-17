using System.Text;
using Konscious.Security.Cryptography;

namespace JsonCrypter.Services;

/// <summary>
/// Provides a method to generate a cryptographic key using the Argon2id algorithm.
/// </summary>
public static class KeyGeneratorService
{
    private const int KeySize = 32; // 256 bits

    /// <summary>
    /// Generates a cryptographic key using the Argon2id algorithm.
    /// </summary>
    /// <param name="password">The password to use for key generation.</param>
    /// <param name="salt">The salt to use for key generation.</param>
    /// <returns>The generated key.</returns>
    public static byte[] GenerateKeyBytes(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt, // 16 bytes
            DegreeOfParallelism = 8, // 8 threads
            MemorySize = 65536, // 64 MB
            Iterations = 4 // 4 passes
        };

        return argon2.GetBytes(KeySize);
    }
}
