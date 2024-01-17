using JsonCrypter.Exceptions;

namespace JsonCrypter.Tests.Exceptions;

public class PathExtensionExceptionTests
{
    [Fact]
    public void ThrowIfNotJson_WithJsonExtension_DoesNotThrow()
    {
        // Arrange
        var filePath = "test.json";

        // Act
        var exception = Record.Exception(() => PathExtensionException.ThrowIfNotJson(filePath));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void ThrowIfNotJson_WithNonJsonExtension_ThrowsException()
    {
        // Arrange
        var filePath = "test.txt";

        // Act
        var exception = Assert.Throws<PathExtensionException>(() => PathExtensionException.ThrowIfNotJson(filePath));

        // Assert
        Assert.Equal("Invalid file type. Please provide a JSON file.", exception.Message);
    }
}
