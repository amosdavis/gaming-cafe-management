using GameCafe.Core.Security;

namespace GameCafe.Tests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ReturnsHashWithThreeParts()
    {
        // Arrange
        const string password = "TestPassword123!";

        // Act
        var hash = _hasher.HashPassword(password);

        // Assert
        var parts = hash.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        const string password = "TestPassword123!";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        const string password = "TestPassword123!";
        const string wrongPassword = "WrongPassword";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void HashPassword_DifferentPasswords_ProduceDifferentHashes()
    {
        // Arrange
        const string password1 = "Password1";
        const string password2 = "Password2";

        // Act
        var hash1 = _hasher.HashPassword(password1);
        var hash2 = _hasher.HashPassword(password2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashPassword_SamePassword_ProducesDifferentHashesDueToSalt()
    {
        // Arrange
        const string password = "TestPassword";

        // Act
        var hash1 = _hasher.HashPassword(password);
        var hash2 = _hasher.HashPassword(password);

        // Assert
        // Due to random salt, hashes should be different
        Assert.NotEqual(hash1, hash2);
        // But both should verify with the same password
        Assert.True(_hasher.VerifyPassword(password, hash1));
        Assert.True(_hasher.VerifyPassword(password, hash2));
    }

    [Fact]
    public void VerifyPassword_WithInvalidHashFormat_ReturnsFalse()
    {
        // Arrange
        const string password = "TestPassword";
        const string invalidHash = "invalid.hash.format.extra";

        // Act
        var isValid = _hasher.VerifyPassword(password, invalidHash);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void HashPassword_EmptyPassword_StillHashes()
    {
        // Arrange
        const string password = "";

        // Act
        var hash = _hasher.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void VerifyPassword_EmptyPassword_Validates()
    {
        // Arrange
        const string password = "";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_VeryLongPassword_WorksCorrectly()
    {
        // Arrange
        var password = new string('a', 500);
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(isValid);
    }
}
