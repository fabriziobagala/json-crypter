using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using JsonCrypter.Models;

namespace JsonCrypter.Helpers;

/// <summary>
/// Provides helper methods for encrypting and decrypting JSON objects using AES-GCM with keys derived from passwords via Argon2id.
/// </summary>
public static class JsonCryptoHelper
{
    private const int SaltSize = 16; // 128 bits

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Encrypts or decrypts every value of the specified JSON object, preserving its structure.
    /// </summary>
    /// <param name="jsonObj">The JSON object whose values are processed.</param>
    /// <param name="password">The password used to derive the cryptographic key.</param>
    /// <param name="operation">The <see cref="Operation"/> to perform on each value.</param>
    /// <returns>A <see cref="string"/> representing the processed JSON object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonObj"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="password"/> is <see langword="null"/>, empty, or consists only of white-space characters.</exception>
    /// <exception cref="FormatException">Thrown during decryption when a value is not a valid Base64 string.</exception>
    /// <exception cref="CryptographicException">Thrown during decryption when the password is incorrect or the data has been tampered with.</exception>
    public static string ProcessJson(JsonObject jsonObj, string password, Operation operation)
    {
        ArgumentNullException.ThrowIfNull(jsonObj);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var processedJson = ProcessNestedValues(jsonObj, password, operation);
        return processedJson.ToJsonString(SerializerOptions);
    }

    /// <summary>
    /// Processes nested values in a JSON node.
    /// </summary>
    /// <param name="node">The JSON node to process.</param>
    /// <param name="password">The password to use for encryption or decryption.</param>
    /// <param name="operation">The operation to perform (encryption or decryption).</param>
    /// <returns>A <see cref="JsonNode"/> representing the processed JSON node.</returns>
    private static JsonNode ProcessNestedValues(JsonNode? node, string password, Operation operation) => node switch
    {
        JsonObject obj => ProcessObject(obj, password, operation),
        JsonArray array => ProcessArray(array, password, operation),
        _ => JsonValue.Create(ExecuteOperation(operation, node?.ToString() ?? string.Empty, password))!
    };

    /// <summary>
    /// Executes the specified operation (encryption or decryption) on the given value using the provided password.
    /// </summary>
    /// <param name="operation">The operation to perform (encryption or decryption).</param>
    /// <param name="value">The value to be encrypted or decrypted.</param>
    /// <param name="password">The password to use for encryption or decryption.</param>
    /// <returns>A <see cref="string"/> representing the result of the operation (encrypted or decrypted value).</returns>
    private static string ExecuteOperation(Operation operation, string value, string password)
    {
        return operation == Operation.Encrypt ? EncryptValue(value, password) : DecryptValue(value, password);
    }

    /// <summary>
    /// Processes a JSON object by performing the specified operation (encryption or decryption) on its nested values.
    /// </summary>
    /// <param name="obj">The JSON object to process.</param>
    /// <param name="password">The password to use for encryption or decryption.</param>
    /// <param name="operation">The operation to perform (encryption or decryption).</param>
    /// <returns>A <see cref="JsonObject"/> representing the processed JSON object.</returns>
    private static JsonObject ProcessObject(JsonObject obj, string password, Operation operation)
    {
        var processedObj = new JsonObject();
        foreach (var property in obj)
        {
            processedObj.Add(property.Key, ProcessNestedValues(property.Value, password, operation));
        }
        return processedObj;
    }

    /// <summary>
    /// Processes a JSON array by performing the specified operation (encryption or decryption) on its nested values.
    /// </summary>
    /// <param name="array">The JSON array to process.</param>
    /// <param name="password">The password to use for encryption or decryption.</param>
    /// <param name="operation">The operation to perform (encryption or decryption).</param>
    /// <returns>A <see cref="JsonArray"/> representing the processed JSON array.</returns>
    private static JsonArray ProcessArray(JsonArray array, string password, Operation operation)
    {
        var processedArray = new JsonArray();
        foreach (var item in array)
        {
            processedArray.Add(ProcessNestedValues(item, password, operation));
        }
        return processedArray;
    }

    /// <summary>
    /// Encrypts the specified value using the provided password.
    /// </summary>
    /// <param name="value">The value to be encrypted.</param>
    /// <param name="password">The password to use for encryption.</param>
    /// <returns>A <see cref="string"/> representing the Base64 string packing the salt, nonce, authentication tag, and ciphertext.</returns>
    /// <remarks>A fresh random salt and nonce are generated on every call.</remarks>
    private static string EncryptValue(string value, string password)
    {
        // Generate a random salt
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Generate a key using the password and salt
        var key = KeyGeneratorHelper.GenerateKeyBytes(password, salt);

        // Encrypt the value
        using var aesGcm = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
        var plaintext = Encoding.UTF8.GetBytes(value);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

        // Combine the salt, nonce, tag, and ciphertext into a single byte array
        var totalLength = salt.Length + nonce.Length + tag.Length + ciphertext.Length;
        var encryptedBytes = new byte[totalLength];

        // Copy the salt, nonce, tag, and ciphertext into the encryptedBytes array
        Buffer.BlockCopy(salt, 0, encryptedBytes, 0, salt.Length);
        Buffer.BlockCopy(nonce, 0, encryptedBytes, salt.Length, nonce.Length);
        Buffer.BlockCopy(tag, 0, encryptedBytes, salt.Length + nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, encryptedBytes, salt.Length + nonce.Length + tag.Length, ciphertext.Length);

        // Return the combined data as a Base64 string
        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Decrypts the specified encrypted value using the provided password.
    /// </summary>
    /// <param name="encryptedValue">The encrypted value to be decrypted, represented as a Base64 string.</param>
    /// <param name="password">The password to use for decryption.</param>
    /// <returns>A <see cref="string"/> representing the decrypted value as a UTF-8 string.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="encryptedValue"/> is not a valid Base64 string.</exception>
    /// <exception cref="CryptographicException">Thrown when the password is incorrect or the data has been tampered with.</exception>
    private static string DecryptValue(string encryptedValue, string password)
    {
        // Convert the encrypted value from a Base64 string to a byte array
        var encryptedBytes = Convert.FromBase64String(encryptedValue);

        // Initialize byte arrays for the salt, nonce, tag, and ciphertext
        var salt = new byte[SaltSize];
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        var cipherText = new byte[encryptedBytes.Length - SaltSize - AesGcm.NonceByteSizes.MaxSize - AesGcm.TagByteSizes.MaxSize];

        // Copy the salt, nonce, tag, and ciphertext from the encrypted bytes into their respective byte arrays
        Buffer.BlockCopy(encryptedBytes, 0, salt, 0, SaltSize);
        Buffer.BlockCopy(encryptedBytes, SaltSize, nonce, 0, AesGcm.NonceByteSizes.MaxSize);
        Buffer.BlockCopy(encryptedBytes, SaltSize + AesGcm.NonceByteSizes.MaxSize, tag, 0, AesGcm.TagByteSizes.MaxSize);
        Buffer.BlockCopy(encryptedBytes, SaltSize + AesGcm.NonceByteSizes.MaxSize + AesGcm.TagByteSizes.MaxSize, cipherText, 0, cipherText.Length);

        // Generate the key from the password and salt
        var key = KeyGeneratorHelper.GenerateKeyBytes(password, salt);

        // Decrypt the ciphertext
        using var aes = new AesGcm(new ReadOnlySpan<byte>(key), AesGcm.TagByteSizes.MaxSize);
        var decryptedBytes = new byte[cipherText.Length];
        aes.Decrypt(nonce, cipherText, tag, decryptedBytes);

        // Return the decrypted data as a UTF8 string
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
