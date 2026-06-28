using JsonCrypter.Models;

namespace JsonCrypter.Tests.Models;

public class OptionsTests
{
    [Theory]
    [InlineData("encrypt", Operation.Encrypt)]
    [InlineData("Encrypt", Operation.Encrypt)]
    [InlineData("ENCRYPT", Operation.Encrypt)]
    [InlineData("decrypt", Operation.Decrypt)]
    [InlineData("Decrypt", Operation.Decrypt)]
    [InlineData("DECRYPT", Operation.Decrypt)]
    public void Operation_WithValidOperationString_ParsesCaseInsensitively(string operationString, Operation expected)
    {
        // Arrange
        var options = new Options { OperationString = operationString };

        // Act
        var result = options.Operation;

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Operation_WithInvalidOperationString_ThrowsArgumentException()
    {
        // Arrange
        var options = new Options { OperationString = "frobnicate" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => options.Operation);
    }
}
