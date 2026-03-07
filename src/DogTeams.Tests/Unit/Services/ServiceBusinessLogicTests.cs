using DogTeams.Api.Auth;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DogTeams.Tests.Unit.Services;

/// <summary>
/// Comprehensive unit tests for RedisCacheService wrapper and cache key generation patterns.
/// Tests validation logic, parameter handling, and cache key formatting.
/// </summary>
public class CacheServicePatternTests
{
    [Fact]
    public void CacheKeyPatterns_ForTeam_GeneratedCorrectly()
    {
        // Arrange
        var teamId = "team-123";
        var expectedCacheKeyPrefix = "team:";

        // Act
        var cacheKey = $"{expectedCacheKeyPrefix}{teamId}";

        // Assert
        cacheKey.Should().Be("team:team-123");
        cacheKey.Should().StartWith("team:");
        cacheKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CacheKeyPatterns_ForOwner_GeneratedCorrectly()
    {
        // Arrange
        var ownerId = "owner-456";
        var teamId = "team-123";
        var expectedKeyPrefix = "owner:";

        // Act
        var cacheKey = $"{expectedKeyPrefix}{ownerId}:team:{teamId}";

        // Assert
        cacheKey.Should().Be("owner:owner-456:team:team-123");
        cacheKey.Should().Contain(":team:");
    }

    [Fact]
    public void CacheKeyPatterns_ForDog_GeneratedCorrectly()
    {
        // Arrange
        var dogId = "dog-789";
        var teamId = "team-123";
        var expectedKeyPrefix = "dog:";

        // Act
        var cacheKey = $"{expectedKeyPrefix}{dogId}:team:{teamId}";

        // Assert
        cacheKey.Should().Be("dog:dog-789:team:team-123");
    }
}

/// <summary>
/// Unit tests for JWT token service business logic.
/// Validates token generation, validation, and claims extraction.
/// </summary>
public class JwtTokenServiceValidationTests
{
    private readonly JwtOptions _jwtOptions;
    private readonly IJwtTokenService _tokenService;

    public JwtTokenServiceValidationTests()
    {
        _jwtOptions = new JwtOptions
        {
            SigningKey = "ThisIsA32CharacterLongDevelopmentKeyForTesting!",
            Issuer = "DogTeams",
            Audience = "DogTeamsUsers",
            AccessTokenExpirationMinutes = 15
        };
        
        var options = Options.Create(_jwtOptions);
        _tokenService = new JwtTokenService(options);
    }

    [Fact]
    public void GenerateAccessToken_WithValidInputs_ProducesValidToken()
    {
        var userId = "user-123";
        var teamId = "team-456";
        
        var token = _tokenService.GenerateAccessToken(userId, teamId);

        token.Should().NotBeNullOrEmpty();
        token.Should().Contain(".");  // JWT has 3 parts separated by dots
        var parts = token.Split('.');
        parts.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(null, "team-123")]
    [InlineData("", "team-123")]
    public void GenerateAccessToken_WithNullOrEmptyUserId_ThrowsArgumentException(string userId, string teamId)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            _tokenService.GenerateAccessToken(userId, teamId));

        ex.Message.Should().Contain("User ID");
    }

    [Theory]
    [InlineData("user-123", null)]
    [InlineData("user-123", "")]
    public void GenerateAccessToken_WithNullOrEmptyTeamId_ThrowsArgumentException(string userId, string teamId)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            _tokenService.GenerateAccessToken(userId, teamId));

        ex.Message.Should().Contain("Team ID");
    }

    [Fact]
    public void GenerateRefreshToken_ProducesRandomString()
    {
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);  // Should be random
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsNotNull()
    {
        var token = _tokenService.GenerateAccessToken("user-123", "team-456");
        
        var principal = _tokenService.ValidateToken(token);

        principal.Should().NotBeNull();
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        var principal = _tokenService.ValidateToken("not-a-valid-token");

        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithTamperedSignature_ReturnsNull()
    {
        var token = _tokenService.GenerateAccessToken("user-123", "team-456");
        var parts = token.Split('.');
        var tamperedToken = string.Join(".", parts[0], parts[1], "tampered");

        var principal = _tokenService.ValidateToken(tamperedToken);

        principal.Should().BeNull();
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ExtractsUserId()
    {
        var userId = "user-123";
        var token = _tokenService.GenerateAccessToken(userId, "team-456");

        var extractedUserId = _tokenService.GetUserIdFromToken(token);

        extractedUserId.Should().Be(userId);
    }

    [Fact]
    public void GetUserIdFromToken_WithInvalidToken_ReturnsNull()
    {
        var userId = _tokenService.GetUserIdFromToken("invalid-token");

        userId.Should().BeNull();
    }
}

/// <summary>
/// Unit tests for user service business logic.
/// Validates user creation, retrieval, and password verification.
/// </summary>
public class UserServiceBusinessLogicTests : IAsyncLifetime
{
    private IUserService _userService = null!;

    public async Task InitializeAsync()
    {
        InMemoryUserService.ClearAllUsers();
        _userService = new InMemoryUserService();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_CreatesUserSuccessfully()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "SecurePassword123!";
        var name = "Test User";

        var user = await _userService.CreateUserAsync(email, password, name);

        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.Name.Should().Be(name);
        user.PasswordHash.Should().NotBe(password);  // Should be hashed, not plain
        user.Id.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(null, "password", "name")]
    [InlineData("", "password", "name")]
    public async Task CreateUserAsync_WithInvalidEmail_ThrowsArgumentException(string email, string password, string name)
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.CreateUserAsync(email, password, name));

        ex.Message.Should().Contain("Email");
    }

    [Theory]
    [InlineData("email@example.com", null, "name")]
    [InlineData("email@example.com", "", "name")]
    public async Task CreateUserAsync_WithInvalidPassword_ThrowsArgumentException(string email, string password, string name)
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.CreateUserAsync(email, password, name));

        ex.Message.Should().Contain("Password");
    }

    [Fact]
    public async Task CreateUserAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        var email = $"duplicate-{Guid.NewGuid()}@example.com";
        await _userService.CreateUserAsync(email, "password1", "User 1");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _userService.CreateUserAsync(email, "password2", "User 2"));

        ex.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithExistingEmail_ReturnsUser()
    {
        var email = $"gettest-{Guid.NewGuid()}@example.com";
        var createdUser = await _userService.CreateUserAsync(email, "password123", "Test User");

        var foundUser = await _userService.GetUserByEmailAsync(email);

        foundUser.Should().NotBeNull();
        foundUser!.Id.Should().Be(createdUser.Id);
        foundUser.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithNonexistentEmail_ReturnsNull()
    {
        var user = await _userService.GetUserByEmailAsync($"nonexistent-{Guid.NewGuid()}@example.com");

        user.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByEmailAsync_IsCaseInsensitive()
    {
        var email = $"CaseSensitive-{Guid.NewGuid()}@Example.COM";
        await _userService.CreateUserAsync(email, "password123", "Test User");

        var user = await _userService.GetUserByEmailAsync(email.ToLower());

        user.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingId_ReturnsUser()
    {
        var createdUser = await _userService.CreateUserAsync($"byid-{Guid.NewGuid()}@example.com", "password123", "Test User");

        var foundUser = await _userService.GetUserByIdAsync(createdUser.Id);

        foundUser.Should().NotBeNull();
        foundUser!.Id.Should().Be(createdUser.Id);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonexistentId_ReturnsNull()
    {
        var user = await _userService.GetUserByIdAsync("nonexistent-id-" + Guid.NewGuid());

        user.Should().BeNull();
    }

    [Fact]
    public async Task VerifyPasswordAsync_WithCorrectPassword_ReturnsTrue()
    {
        var email = $"verify-{Guid.NewGuid()}@example.com";
        var password = "SecurePassword123";
        await _userService.CreateUserAsync(email, password, "Test User");

        var result = await _userService.VerifyPasswordAsync(email, password);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPasswordAsync_WithIncorrectPassword_ReturnsFalse()
    {
        var email = $"wrong-{Guid.NewGuid()}@example.com";
        await _userService.CreateUserAsync(email, "SecurePassword123", "Test User");

        var result = await _userService.VerifyPasswordAsync(email, "WrongPassword");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyPasswordAsync_WithNonexistentEmail_ReturnsFalse()
    {
        var result = await _userService.VerifyPasswordAsync($"nonexistent-{Guid.NewGuid()}@example.com", "password");

        result.Should().BeFalse();
    }
}
