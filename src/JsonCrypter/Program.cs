using System.ComponentModel;
using CommandLine;
using JsonCrypter.Exceptions;
using JsonCrypter.Models;
using JsonCrypter.Services;
using Newtonsoft.Json.Linq;

Parser.Default.ParseArguments<Options>(args)
    .WithParsed(RunWithOptions);

/// <summary>
/// Runs the application with the specified options.
/// </summary>
/// <param name="opts">The options to run the application with.</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="opts"/> is null.</exception>
/// <exception cref="ArgumentException">Thrown when any of the properties of <paramref name="opts"/> are null, empty, or consists only of white-space characters.</exception>
/// <exception cref="PathExtensionException">Thrown when the file path in <paramref name="opts"/> does not have a .json extension.</exception>
/// <exception cref="InvalidEnumArgumentException">Thrown when the operation in <paramref name="opts"/> is not a valid <see cref="Operation"/>.</exception>
static void RunWithOptions(Options opts)
{
    ArgumentNullException.ThrowIfNull(opts);

    ArgumentException.ThrowIfNullOrWhiteSpace(opts.OperationString);
    ArgumentException.ThrowIfNullOrWhiteSpace(opts.FilePath);
    ArgumentException.ThrowIfNullOrWhiteSpace(opts.Password);

    PathExtensionException.ThrowIfNotJson(opts.FilePath);

    var jsonString = File.ReadAllText(opts.FilePath);
    var jsonObject = JObject.Parse(jsonString);

    switch (opts.Operation)
    {
        case Operation.Encrypt:
            var encryptedJson = JsonCryptoService.ProcessJson(jsonObject, opts.Password, Operation.Encrypt);
            File.WriteAllText(opts.FilePath, encryptedJson);
            Console.WriteLine($"Encrypted file has been saved to {opts.FilePath}");
            break;

        case Operation.Decrypt:
            var decryptedJson = JsonCryptoService.ProcessJson(jsonObject, opts.Password, Operation.Decrypt);
            File.WriteAllText(opts.FilePath, decryptedJson);
            Console.WriteLine($"Decrypted file has been saved to {opts.FilePath}");
            break;

        default:
            throw new InvalidEnumArgumentException(nameof(opts.Operation), (int)opts.Operation, typeof(Operation));
    }
}
