using System.ComponentModel;
using CommandLine;
using JsonCrypter.Exceptions;
using JsonCrypter.Models;
using JsonCrypter.Helpers;
using System.Text.Json.Nodes;

Parser.Default.ParseArguments<Options>(args)
    .WithParsed(RunWithOptions);

static void RunWithOptions(Options opts)
{
    ArgumentNullException.ThrowIfNull(opts);

    ArgumentException.ThrowIfNullOrWhiteSpace(opts.OperationString);
    ArgumentException.ThrowIfNullOrWhiteSpace(opts.FilePath);
    ArgumentException.ThrowIfNullOrWhiteSpace(opts.Password);

    PathExtensionException.ThrowIfNotJson(opts.FilePath);

    var jsonString = File.ReadAllText(opts.FilePath);
    var jsonObject = JsonNode.Parse(jsonString)!.AsObject();

    switch (opts.Operation)
    {
        case Operation.Encrypt:
            var encryptedJson = JsonCryptoHelper.ProcessJson(jsonObject, opts.Password, Operation.Encrypt);
            File.WriteAllText(opts.FilePath, encryptedJson);
            Console.WriteLine($"Encrypted file has been saved to {opts.FilePath}");
            break;

        case Operation.Decrypt:
            var decryptedJson = JsonCryptoHelper.ProcessJson(jsonObject, opts.Password, Operation.Decrypt);
            File.WriteAllText(opts.FilePath, decryptedJson);
            Console.WriteLine($"Decrypted file has been saved to {opts.FilePath}");
            break;

        default:
            throw new InvalidEnumArgumentException(nameof(opts.Operation), (int)opts.Operation, typeof(Operation));
    }
}
