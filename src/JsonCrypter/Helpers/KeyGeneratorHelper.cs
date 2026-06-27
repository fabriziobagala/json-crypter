using System.Text;
using Konscious.Security.Cryptography;

namespace JsonCrypter.Helpers;

/// <summary>
/// Provides helper methods for generating cryptographic keys from passwords and salts.
/// </summary>
public static class KeyGeneratorHelper
{
    private const int KeySize = 32; // 256 bits

    /// <summary>
    /// Derives a cryptographic key from a password and salt using the Argon2id algorithm.
    /// </summary>
    /// <param name="password">The password to derive the key from.</param>
    /// <param name="salt">The salt to derive the key from; a 16-byte (128-bit) value is expected.</param>
    /// <returns>A <see cref="byte"/> array representing the derived 256-bit (32-byte) key.</returns>
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
