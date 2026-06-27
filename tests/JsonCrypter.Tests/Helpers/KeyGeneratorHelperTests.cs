using System.Text;
using JsonCrypter.Helpers;

namespace JsonCrypter.Tests.Helpers;

public class KeyGeneratorHelperTests
{
    [Fact]
    public void GenerateKeyBytes_ValidInput_ReturnsByteArrayOfCorrectLength()
    {
        // Arrange
        const int expectedKeySizeInBytes = 32;
        var password = "testPassword";
        var salt = Encoding.UTF8.GetBytes("testSalt");

        // Act
        var result = KeyGeneratorHelper.GenerateKeyBytes(password, salt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedKeySizeInBytes, result.Length);
    }

    [Fact]
    public void GenerateKeyBytes_WithSameInputs_ReturnsSameKey()
    {
        // Arrange
        var password = "testPassword";
        var salt = Encoding.UTF8.GetBytes("testSalt");

        // Act
        var first = KeyGeneratorHelper.GenerateKeyBytes(password, salt);
        var second = KeyGeneratorHelper.GenerateKeyBytes(password, salt);

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
        var key1 = KeyGeneratorHelper.GenerateKeyBytes(password, salt1);
        var key2 = KeyGeneratorHelper.GenerateKeyBytes(password, salt2);

        // Assert
        Assert.NotEqual(key1, key2);
    }
}
