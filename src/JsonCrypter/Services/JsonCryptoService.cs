using System.Security.Cryptography;
using System.Text;
using JsonCrypter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonCrypter.Services;

/// <summary>
/// Provides methods for encrypting and decrypting JSON data.
/// </summary>
public static class JsonCryptoService
{
    private const int SaltSize = 16; // 128 bits

    /// <summary>
    /// Processes a JSON object by either encrypting or decrypting its values based on the specified operation.
    /// </summary>
    /// <param name="jsonObj">The JSON object to process.</param>
    /// <param name="password">The password to use for encryption or decryption.</param>
    /// <param name="operation">The operation to perform, either encryption or decryption.</param>
    /// <returns>The processed JSON object as a string with indented formatting.</returns>
    /// <exception cref="ArgumentNullException">Thrown when jsonObj is null.</exception>
    /// <exception cref="ArgumentException">Thrown when password is null, empty, or consists only of white-space characters.</exception>
    public static string ProcessJson(JObject jsonObj, string password, Operation operation)
    {
        ArgumentNullException.ThrowIfNull(jsonObj);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var encryptedJson = ProcessNestedValues(jsonObj, password, operation);
        return encryptedJson.ToString(Formatting.Indented);
    }

    /// <summary>
    /// Processes nested values in a JSON token.
    /// </summary>
    /// <param name="token">The JSON token to process.</param>
    /// <param name="password">The password to use for encryption or decryption.</param>
    /// <param name="operation">The operation to perform (encryption or decryption).</param>
    /// <returns>The processed JSON token.</returns>
    private static JToken ProcessNestedValues(JToken token, string password, Operation operation) => token.Type switch
    {
        JTokenType.Object => ProcessObject((JObject)token, password, operation),
        JTokenType.Array => ProcessArray((JArray)token, password, operation),
        _ => (JToken)ExecuteOperation(operation, token.ToString(), password)
    };

    /// <summary>
    /// Executes the specified operation (encryption or decryption) on the given value using the provided password.
    /// </summary>
    /// <param name="operation">The operation to perform (encryption or decryption).</param>
    /// <param name="value">The value to be encrypted or decrypted.</param>
    /// <param name="password">The password to use for encryption or decryption.</param>
    /// <returns>The result of the operation (encrypted or decrypted value).</returns>
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
    /// <returns>A new JSON object with the processed nested values.</returns>
    private static JObject ProcessObject(JObject obj, string password, Operation operation)
    {
        var processedObj = new JObject();
        foreach (var property in obj.Properties())
        {
            processedObj.Add(property.Name, ProcessNestedValues(property.Value, password, operation));
        }
        return processedObj;
    }

    /// <summary>
    /// Processes a JSON array by performing the specified operation (encryption or decryption) on its nested values.
    /// </summary>
    /// <param name="array">The JSON array to process.</param>
    /// <param name="password">The password to use for encryption or decryption.</param>
    /// <param name="operation">The operation to perform (encryption or decryption).</param>
    /// <returns>A new JSON array with the processed nested values.</returns>
    private static JArray ProcessArray(JArray array, string password, Operation operation)
    {
        var processedArray = new JArray();
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
    /// <returns>The encrypted value as a Base64 string.</returns>
    private static string EncryptValue(string value, string password)
    {
        // Generate a random salt
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Generate a key using the password and salt
        var key = KeyGeneratorService.GenerateKeyBytes(password, salt);

        // Encrypt the value
        using var aesGcm = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
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
    /// <returns>The decrypted value as a UTF8 string.</returns>
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
        var key = KeyGeneratorService.GenerateKeyBytes(password, salt);

        // Decrypt the ciphertext
        using var aes = new AesGcm(new ReadOnlySpan<byte>(key), AesGcm.TagByteSizes.MaxSize);
        var decryptedBytes = new byte[cipherText.Length];
        aes.Decrypt(nonce, cipherText, tag, decryptedBytes);

        // Return the decrypted data as a UTF8 string
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
