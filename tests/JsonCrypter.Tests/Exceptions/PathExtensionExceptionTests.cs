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
    public void ThrowIfNotJson_WithUppercaseJsonExtension_DoesNotThrow()
    {
        // Arrange
        var filePath = "test.JSON";

        // Act
        var exception = Record.Exception(() => PathExtensionException.ThrowIfNotJson(filePath));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void ThrowIfNotJson_WithNonJsonExtension_ThrowsPathExtensionException()
    {
        // Arrange
        var filePath = "test.txt";

        // Act
        var exception = Assert.Throws<PathExtensionException>(() => PathExtensionException.ThrowIfNotJson(filePath));

        // Assert
        Assert.Equal("Invalid file type. Please provide a JSON file.", exception.Message);
    }

    [Fact]
    public void ThrowIfNotJson_WithNoExtension_ThrowsPathExtensionException()
    {
        // Arrange
        var filePath = "test";

        // Act
        var exception = Assert.Throws<PathExtensionException>(() => PathExtensionException.ThrowIfNotJson(filePath));

        // Assert
        Assert.Equal("Invalid file type. Please provide a JSON file.", exception.Message);
    }

    [Fact]
    public void ThrowIfNotJson_WithJsonNotAsLastExtension_ThrowsPathExtensionException()
    {
        // Arrange
        var filePath = "test.json.bak";

        // Act
        var exception = Assert.Throws<PathExtensionException>(() => PathExtensionException.ThrowIfNotJson(filePath));

        // Assert
        Assert.Equal("Invalid file type. Please provide a JSON file.", exception.Message);
    }

    [Fact]
    public void Constructor_WithInnerException_PreservesMessageAndInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("inner");

        // Act
        var exception = new PathExtensionException("outer", innerException);

        // Assert
        Assert.Equal("outer", exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }
}
