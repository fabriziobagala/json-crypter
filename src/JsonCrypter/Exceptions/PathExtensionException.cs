namespace JsonCrypter.Exceptions;

/// <summary>
/// Represents errors that occur due to invalid file extensions in a path.
/// </summary>
public class PathExtensionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PathExtensionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PathExtensionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PathExtensionException"/> class with a specified error message and 
    /// a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public PathExtensionException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Throws a <see cref="PathExtensionException"/> if the file extension of the provided path is not .json.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    public static void ThrowIfNotJson(string filePath)
    {
        if (Path.GetExtension(filePath) != ".json")
        {
            throw new PathExtensionException("Invalid file type. Please provide a JSON file.");
        }
    }
}
