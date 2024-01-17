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
}
