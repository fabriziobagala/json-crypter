using System.Security.Cryptography;
using System.Text.Json.Nodes;
using JsonCrypter.Models;
using JsonCrypter.Helpers;

namespace JsonCrypter.Tests.Helpers;

public class JsonCryptoHelperTests
{
    [Fact]
    public void ProcessJson_NullJsonObj_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => JsonCryptoHelper.ProcessJson(null!, "password", Operation.Encrypt));
    }

    [Fact]
    public void ProcessJson_NullPassword_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => JsonCryptoHelper.ProcessJson([], null!, Operation.Encrypt));
    }

    [Fact]
    public void ProcessJson_EmptyPassword_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => JsonCryptoHelper.ProcessJson([], "", Operation.Encrypt));
    }

    [Fact]
    public void ProcessJson_WhitespacePassword_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => JsonCryptoHelper.ProcessJson([], " ", Operation.Encrypt));
    }

    [Fact]
    public void ProcessJson_EncryptOperation_EncryptsJson()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "value"
        };

        // Act
        var result = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);

        // Assert
        Assert.NotEqual(jsonObj.ToString(), result);
        Assert.DoesNotContain("value", result);
    }

    [Fact]
    public void ProcessJson_DecryptOperation_DecryptsJson()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "value"
        };
        var encrypted = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);

        // Act
        var decrypted = JsonCryptoHelper.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "password", Operation.Decrypt);

        // Assert
        var result = JsonNode.Parse(decrypted)!.AsObject();
        Assert.Equal("value", result["test"]!.GetValue<string>());
    }

    [Fact]
    public void ProcessJson_EncryptOperation_ReturnsIndentedJson()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "value"
        };

        // Act
        var result = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);

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
        var encrypted = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var decrypted = JsonCryptoHelper.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "password", Operation.Decrypt);

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
        var encrypted = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var decrypted = JsonCryptoHelper.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "password", Operation.Decrypt);

        // Assert
        var result = JsonNode.Parse(decrypted)!.AsObject();
        Assert.Equal("30", result["age"]!.GetValue<string>());
    }

    [Fact]
    public void ProcessJson_DecryptWithWrongPassword_ThrowsCryptographicException()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "value"
        };
        var encrypted = JsonCryptoHelper.ProcessJson(jsonObj, "correct-password", Operation.Encrypt);

        // Act & Assert
        Assert.ThrowsAny<CryptographicException>(() =>
            JsonCryptoHelper.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "wrong-password", Operation.Decrypt));
    }

    [Fact]
    public void ProcessJson_DecryptWithTamperedCiphertext_ThrowsCryptographicException()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "value"
        };
        var encrypted = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var encObj = JsonNode.Parse(encrypted)!.AsObject();

        var bytes = Convert.FromBase64String(encObj["test"]!.GetValue<string>());
        bytes[^1] ^= 0xFF;
        encObj["test"] = Convert.ToBase64String(bytes);

        // Act & Assert
        Assert.ThrowsAny<CryptographicException>(() =>
            JsonCryptoHelper.ProcessJson(encObj, "password", Operation.Decrypt));
    }

    [Fact]
    public void ProcessJson_DecryptWithNonBase64Value_ThrowsFormatException()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "not-valid-base64!!!"
        };

        // Act & Assert
        Assert.Throws<FormatException>(() =>
            JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Decrypt));
    }

    [Fact]
    public void ProcessJson_DecryptWithTruncatedPayload_ThrowsOverflowException()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = Convert.ToBase64String(new byte[10])
        };

        // Act & Assert (cipherText length goes negative -> overflow when sizing the buffer)
        Assert.Throws<OverflowException>(() =>
            JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Decrypt));
    }

    [Fact]
    public void ProcessJson_EncryptOperation_UsesNonZeroNonce()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "value"
        };

        // Act
        var encrypted = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var payload = Convert.FromBase64String(JsonNode.Parse(encrypted)!.AsObject()["test"]!.GetValue<string>());

        // Assert: the 12-byte nonce stored after the 16-byte salt must not be all zeros
        const int saltSize = 16;
        const int nonceSize = 12;
        var nonce = payload[saltSize..(saltSize + nonceSize)];
        Assert.Contains(nonce, b => b != 0);
    }
}
