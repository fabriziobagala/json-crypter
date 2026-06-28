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

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ProcessJson_EmptyOrWhitespacePassword_ThrowsArgumentException(string password)
    {
        Assert.Throws<ArgumentException>(() => JsonCryptoHelper.ProcessJson([], password, Operation.Encrypt));
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

        // Act & Assert
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

        // Assert
        var saltSize = JsonCryptoHelper.SaltSize;
        var nonceSize = AesGcm.NonceByteSizes.MaxSize;
        var nonce = payload[saltSize..(saltSize + nonceSize)];
        Assert.Contains(nonce, b => b != 0);
    }

    [Fact]
    public void ProcessJson_EncryptingSameValueTwice_ProducesDifferentCiphertext()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = "value"
        };

        // Act
        var first = JsonNode.Parse(JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt))!
            .AsObject()["test"]!.GetValue<string>();
        var second = JsonNode.Parse(JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt))!
            .AsObject()["test"]!.GetValue<string>();

        // Assert
        Assert.NotEqual(first, second);

        // The randomness lives in the salt (first bytes) and nonce that follow it, so the
        // leading salt+nonce region of the two payloads must differ.
        var saltSize = JsonCryptoHelper.SaltSize;
        var nonceSize = AesGcm.NonceByteSizes.MaxSize;
        var firstPrefix = Convert.FromBase64String(first)[..(saltSize + nonceSize)];
        var secondPrefix = Convert.FromBase64String(second)[..(saltSize + nonceSize)];
        Assert.NotEqual(firstPrefix, secondPrefix);
    }

    [Fact]
    public void ProcessJson_RoundTrip_PreservesArrayOfObjects()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["people"] = new JsonArray
            {
                new JsonObject { ["name"] = "Alice" },
                new JsonObject { ["name"] = "Bob" }
            }
        };

        // Act
        var encrypted = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var decrypted = JsonCryptoHelper.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "password", Operation.Decrypt);

        // Assert
        var result = JsonNode.Parse(decrypted)!.AsObject();
        Assert.Equal("Alice", result["people"]![0]!["name"]!.GetValue<string>());
        Assert.Equal("Bob", result["people"]![1]!["name"]!.GetValue<string>());
    }

    [Fact]
    public void ProcessJson_RoundTrip_PreservesNestedArrays()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["matrix"] = new JsonArray
            {
                new JsonArray { "a", "b" },
                new JsonArray { "c" }
            }
        };

        // Act
        var encrypted = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var decrypted = JsonCryptoHelper.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "password", Operation.Decrypt);

        // Assert
        var result = JsonNode.Parse(decrypted)!.AsObject();
        Assert.Equal("a", result["matrix"]![0]![0]!.GetValue<string>());
        Assert.Equal("b", result["matrix"]![0]![1]!.GetValue<string>());
        Assert.Equal("c", result["matrix"]![1]![0]!.GetValue<string>());
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void ProcessJson_RoundTrip_ConvertsBooleanValuesToStrings(bool value, string expected)
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["flag"] = value
        };

        // Act
        var encrypted = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var decrypted = JsonCryptoHelper.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "password", Operation.Decrypt);

        // Assert
        var result = JsonNode.Parse(decrypted)!.AsObject();
        Assert.Equal(expected, result["flag"]!.GetValue<string>());
    }

    [Fact]
    public void ProcessJson_EncryptOperation_PreservesEmptyObject()
    {
        // Arrange
        var jsonObj = new JsonObject();

        // Act
        var result = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);

        // Assert
        var resultObj = JsonNode.Parse(result)!.AsObject();
        Assert.Empty(resultObj);
    }

    [Fact]
    public void ProcessJson_RoundTrip_PreservesEmptyArray()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["items"] = new JsonArray()
        };

        // Act
        var encrypted = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var decrypted = JsonCryptoHelper.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "password", Operation.Decrypt);

        // Assert
        var result = JsonNode.Parse(decrypted)!.AsObject();
        Assert.Empty(result["items"]!.AsArray());
    }

    [Fact]
    public void ProcessJson_RoundTrip_PreservesEmptyStringValue()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = ""
        };

        // Act
        var encrypted = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var decrypted = JsonCryptoHelper.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "password", Operation.Decrypt);

        // Assert
        var result = JsonNode.Parse(decrypted)!.AsObject();
        Assert.Equal("", result["test"]!.GetValue<string>());
    }

    [Fact]
    public void ProcessJson_RoundTrip_ConvertsNullValueToEmptyString()
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = null
        };

        // Act
        var encrypted = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var decrypted = JsonCryptoHelper.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "password", Operation.Decrypt);

        // Assert
        var result = JsonNode.Parse(decrypted)!.AsObject();
        Assert.Equal("", result["test"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("Café au lait")]
    [InlineData("日本語のテキスト")]
    [InlineData("emoji 😀🔐🚀")]
    public void ProcessJson_RoundTrip_PreservesMultibyteUtf8Values(string value)
    {
        // Arrange
        var jsonObj = new JsonObject
        {
            ["test"] = value
        };

        // Act
        var encrypted = JsonCryptoHelper.ProcessJson(jsonObj, "password", Operation.Encrypt);
        var decrypted = JsonCryptoHelper.ProcessJson(JsonNode.Parse(encrypted)!.AsObject(), "password", Operation.Decrypt);

        // Assert
        var result = JsonNode.Parse(decrypted)!.AsObject();
        Assert.Equal(value, result["test"]!.GetValue<string>());
    }
}
