using CommandLine;

namespace JsonCrypter.Models;

/// <summary>
/// Represents the options for the JSON Crypter application.
/// </summary>
public record Options
{
    /// <summary>
    /// Gets or sets the operation to perform.
    /// </summary>
    /// <value>
    /// The operation as a string.
    /// </value>
    [Option('o', "operation", Required = true)]
    public string? OperationString { get; set; }

    /// <summary>
    /// Gets the operation to perform.
    /// </summary>
    /// <value>
    /// The operation as an <see cref="Operation"/> enum.
    /// </value>
    public Operation Operation => Enum.Parse<Operation>(OperationString!, true);

    /// <summary>
    /// Gets or sets the file path of the file to process.
    /// </summary>
    /// <value>
    /// The file path as a string.
    /// </value>
    [Option('f', "file", Required = true)]
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the password to use for encryption or decryption.
    /// </summary>
    /// <value>
    /// The password as a string.
    /// </value>
    [Option('p', "password", Required = true)]
    public string? Password { get; set; }
}
