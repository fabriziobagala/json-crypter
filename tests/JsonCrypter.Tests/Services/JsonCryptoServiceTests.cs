using JsonCrypter.Models;
using JsonCrypter.Services;
using System.Text.Json.Nodes;

namespace JsonCrypter.Tests.Services;

public class JsonCryptoServiceTests
{
    [Fact]
    public void ProcessJson_ThrowsArgumentNullException_WhenJsonObjIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => JsonCryptoService.ProcessJson(null!, "password", Operation.Encrypt));
    }

    [Fact]
    public void ProcessJson_ThrowsArgumentNullException_WhenPasswordIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => JsonCryptoService.ProcessJson(new JsonObject(),null!, Operation.Encrypt));
    }

    [Fact]
    public void ProcessJson_ThrowsArgumentException_WhenPasswordIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => JsonCryptoService.ProcessJson(new JsonObject(),"", Operation.Encrypt));
    }

    [Fact]
    public void ProcessJson_ThrowsArgumentException_WhenPasswordIsWhitespace()
    {
        Assert.Throws<ArgumentException>(() => JsonCryptoService.ProcessJson(new JsonObject()," ", Operation.Encrypt));
    }

    [Fact]
    public void ProcessJson_EncryptsJson_WhenOperationIsEncrypt()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "value"
        };

        // Act
        var result = JsonCryptoService.ProcessJson(jsonObj, "password", Operation.Encrypt);

        // Assert
        Assert.NotEqual(jsonObj.ToString(), result);
    }

    [Fact]
    public void ProcessJson_DecryptsJson_WhenOperationIsDecrypt()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "value"
        };
        var encrypted = JsonCryptoService.ProcessJson(jsonObj, "password", Operation.Encrypt);

        // Act
        var decrypted = JsonCryptoService.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "password", Operation.Decrypt);

        // Assert
        Assert.Equal(jsonObj.ToString(), decrypted);
    }

    [Fact]
    public void ProcessJson_ReturnsIndentedJson()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "value"
        };

        // Act
        var result = JsonCryptoService.ProcessJson(jsonObj, "password", Operation.Encrypt);

        // Assert
        Assert.Contains("\n", result);
    }
}
