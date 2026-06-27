using System.Security.Cryptography;
using System.Text.Json.Nodes;
using JsonCrypter.Models;
using JsonCrypter.Services;

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
        Assert.DoesNotContain("value", result);
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
        var result = JsonNode.Parse(decrypted)!.AsObject();
        Assert.Equal("value", result["test"]!.GetValue<string>());
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

    [Fact]
    public void ProcessJson_RoundTrip_PreservesNestedObjectsAndArrays()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["name"] = "Alice",
            ["tags"] = new JsonArray { "a", "b" },
            ["nested"] = new JsonObject { ["city"] = "Rome" }
        };

        // Act
        var encrypted = JsonCryptoService.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var decrypted = JsonCryptoService.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "password", Operation.Decrypt);

        // Assert
        var result = JsonNode.Parse(decrypted)!.AsObject();
        Assert.Equal("Alice", result["name"]!.GetValue<string>());
        Assert.Equal("a", result["tags"]![0]!.GetValue<string>());
        Assert.Equal("b", result["tags"]![1]!.GetValue<string>());
        Assert.Equal("Rome", result["nested"]!["city"]!.GetValue<string>());
    }

    [Fact]
    public void ProcessJson_RoundTrip_ConvertsNonStringValuesToStrings()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["age"] = 30
        };

        // Act
        var encrypted = JsonCryptoService.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var decrypted = JsonCryptoService.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "password", Operation.Decrypt);

        // Assert
        var result = JsonNode.Parse(decrypted)!.AsObject();
        Assert.Equal("30", result["age"]!.GetValue<string>());
    }

    [Fact]
    public void ProcessJson_Decrypt_WithWrongPassword_ThrowsCryptographicException()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "value"
        };
        var encrypted = JsonCryptoService.ProcessJson(jsonObj, "correct-password", Operation.Encrypt);

        // Act & Assert
        Assert.ThrowsAny<CryptographicException>(() =>
            JsonCryptoService.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "wrong-password", Operation.Decrypt));
    }

    [Fact]
    public void ProcessJson_Decrypt_WithTamperedCiphertext_ThrowsCryptographicException()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "value"
        };
        var encrypted = JsonCryptoService.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var encObj = JsonNode.Parse(encrypted)!.AsObject();

        var bytes = Convert.FromBase64String(encObj["test"]!.GetValue<string>());
        bytes[^1] ^= 0xFF;
        encObj["test"] = Convert.ToBase64String(bytes);

        // Act & Assert
        Assert.ThrowsAny<CryptographicException>(() =>
            JsonCryptoService.ProcessJson(encObj, "password", Operation.Decrypt));
    }

    [Fact]
    public void ProcessJson_Decrypt_WithNonBase64Value_ThrowsFormatException()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "not-valid-base64!!!"
        };

        // Act & Assert
        Assert.Throws<FormatException>(() =>
            JsonCryptoService.ProcessJson(jsonObj, "password", Operation.Decrypt));
    }

    [Fact]
    public void ProcessJson_Decrypt_WithTruncatedPayload_Throws()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = Convert.ToBase64String(new byte[10])
        };

        // Act & Assert
        Assert.ThrowsAny<Exception>(() =>
            JsonCryptoService.ProcessJson(jsonObj, "password", Operation.Decrypt));
    }
}
