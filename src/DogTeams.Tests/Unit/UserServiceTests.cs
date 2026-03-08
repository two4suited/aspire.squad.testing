using DogTeams.Api.Auth;
using FluentAssertions;

namespace DogTeams.Tests.Unit;

public class UserServiceTests : IAsyncLifetime
{
    private IUserService _userService = null!;

    public async Task InitializeAsync()
    {
        // Clear static user store before each test to ensure isolation
        InMemoryUserService.ClearAllUsers();
        _userService = new InMemoryUserService();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_CreatesUser()
    {
        // Arrange
        var email = $"test{Guid.NewGuid()}@example.com";
        var password = "SecurePassword123";
        var name = "Test User";

        // Act
        var user = await _userService.CreateUserAsync(email, password, name);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.Name.Should().Be(name);
        user.PasswordHash.Should().NotBeNullOrEmpty();
        user.PasswordHash.Should().NotBe(password);  // Should be hashed, not plain
    }

    [Fact]
    public async Task CreateUserAsync_WithDuplicateEmail_ThrowsException()
    {
        // Arrange
        var email = $"duplicate{Guid.NewGuid()}@example.com";
        var password = "SecurePassword123";
        await _userService.CreateUserAsync(email, password, "User 1");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _userService.CreateUserAsync(email, "DifferentPassword", "User 2"));
        
        ex.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateUserAsync_WithNullEmail_ThrowsException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.CreateUserAsync(null!, "password", "name"));
        
        ex.Message.Should().Contain("Email");
    }

    [Fact]
    public async Task CreateUserAsync_WithNullPassword_ThrowsException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.CreateUserAsync("test@example.com", null!, "name"));
        
        ex.Message.Should().Contain("Password");
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithExistingEmail_ReturnsUser()
    {
        // Arrange
        var email = $"gettest{Guid.NewGuid()}@example.com";
        var password = "SecurePassword123";
        var createdUser = await _userService.CreateUserAsync(email, password, "Test User");

        // Act
        var foundUser = await _userService.GetUserByEmailAsync(email);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Id.Should().Be(createdUser.Id);
        foundUser.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithNonexistentEmail_ReturnsNull()
    {
        // Act
        var user = await _userService.GetUserByEmailAsync($"nonexistent{Guid.NewGuid()}@example.com");

        // Assert
        user.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByEmailAsync_IsCaseInsensitive()
    {
        // Arrange
        var email = $"CaseSensitive{Guid.NewGuid()}@Example.COM";
        await _userService.CreateUserAsync(email, "password123", "Test User");

        // Act
        var user = await _userService.GetUserByEmailAsync(email.ToLower());

        // Assert
        user.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingId_ReturnsUser()
    {
        // Arrange
        var createdUser = await _userService.CreateUserAsync($"byid{Guid.NewGuid()}@example.com", "password123", "Test User");

        // Act
        var foundUser = await _userService.GetUserByIdAsync(createdUser.Id);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Id.Should().Be(createdUser.Id);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonexistentId_ReturnsNull()
    {
        // Act
        var user = await _userService.GetUserByIdAsync("nonexistent-id-" + Guid.NewGuid());

        // Assert
        user.Should().BeNull();
    }

    [Fact]
    public async Task VerifyPasswordAsync_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var email = $"verify{Guid.NewGuid()}@example.com";
        var password = "SecurePassword123";
        await _userService.CreateUserAsync(email, password, "Test User");

        // Act
        var result = await _userService.VerifyPasswordAsync(email, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPasswordAsync_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var email = $"wrong{Guid.NewGuid()}@example.com";
        await _userService.CreateUserAsync(email, "SecurePassword123", "Test User");

        // Act
        var result = await _userService.VerifyPasswordAsync(email, "WrongPassword");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyPasswordAsync_WithNonexistentEmail_ReturnsFalse()
    {
        // Act
        var result = await _userService.VerifyPasswordAsync($"nonexistent{Guid.NewGuid()}@example.com", "password");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task MultipleUsers_CanBeCreatedAndRetrieved()
    {
        // Arrange & Act
        var user1 = await _userService.CreateUserAsync($"multi1{Guid.NewGuid()}@example.com", "password1", "User 1");
        var user2 = await _userService.CreateUserAsync($"multi2{Guid.NewGuid()}@example.com", "password2", "User 2");

        // Assert
        var foundUser1 = await _userService.GetUserByIdAsync(user1.Id);
        var foundUser2 = await _userService.GetUserByIdAsync(user2.Id);

        foundUser1.Should().NotBeNull();
        foundUser2.Should().NotBeNull();
        foundUser1!.Id.Should().Be(user1.Id);
        foundUser2!.Id.Should().Be(user2.Id);
    }

    [Fact]
    public async Task SeedTestUsers_CreatesTestUser_WithCorrectCredentials()
    {
        // Arrange
        InMemoryUserService.ClearAllUsers();
        
        // Act
        InMemoryUserService.SeedTestUsers();
        
        // Assert - Test user should exist
        var user = await _userService.GetUserByEmailAsync("test@example.com");
        user.Should().NotBeNull();
        user!.Email.Should().Be("test@example.com");
        user.Name.Should().Be("Test User");
        
        // Password verification should work
        var pwVerified = await _userService.VerifyPasswordAsync("test@example.com", "TestPassword123!");
        pwVerified.Should().BeTrue("test user password should verify correctly");
        
        // Wrong password should fail
        var wrongPwVerified = await _userService.VerifyPasswordAsync("test@example.com", "WrongPassword");
        wrongPwVerified.Should().BeFalse("wrong password should not verify");
    }

    [Fact]
    public async Task SeedTestUsers_IsIdempotent_DoesNotDuplicateOnMultipleCalls()
    {
        // Arrange
        InMemoryUserService.ClearAllUsers();
        
        // Act
        InMemoryUserService.SeedTestUsers();
        InMemoryUserService.SeedTestUsers();  // Call again
        
        // Assert - Should still be only one test user
        var user = await _userService.GetUserByEmailAsync("test@example.com");
        user.Should().NotBeNull();
        
        var verifyResult = await _userService.VerifyPasswordAsync("test@example.com", "TestPassword123!");
        verifyResult.Should().BeTrue("test user should still be able to login after multiple seed calls");
    }
}
