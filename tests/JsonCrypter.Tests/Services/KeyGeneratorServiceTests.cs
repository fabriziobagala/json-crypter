using System.Text;
using JsonCrypter.Services;

namespace JsonCrypter.Tests.Services;

public class KeyGeneratorServiceTests
{
    [Fact]
    public void GenerateKeyBytes_ShouldReturnByteArrayOfCorrectLength()
    {
        // Arrange
        var password = "testPassword";
        var salt = Encoding.UTF8.GetBytes("testSalt");
        var expectedLength = 32;

        // Act
        var result = KeyGeneratorService.GenerateKeyBytes(password, salt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedLength, result.Length);
    }

    [Fact]
    public void GenerateKeyBytes_WithSameInputs_ReturnsSameKey()
    {
        // Arrange
        var password = "testPassword";
        var salt = Encoding.UTF8.GetBytes("testSalt");

        // Act
        var first = KeyGeneratorService.GenerateKeyBytes(password, salt);
        var second = KeyGeneratorService.GenerateKeyBytes(password, salt);

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void GenerateKeyBytes_WithDifferentSalt_ReturnsDifferentKey()
    {
        // Arrange
        var password = "testPassword";
        var salt1 = Encoding.UTF8.GetBytes("testSalt1");
        var salt2 = Encoding.UTF8.GetBytes("testSalt2");

        // Act
        var key1 = KeyGeneratorService.GenerateKeyBytes(password, salt1);
        var key2 = KeyGeneratorService.GenerateKeyBytes(password, salt2);

        // Assert
        Assert.NotEqual(key1, key2);
    }
}
