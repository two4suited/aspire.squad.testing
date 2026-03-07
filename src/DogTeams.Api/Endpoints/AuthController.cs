using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DogTeams.Api.Auth;
using DogTeams.Api.DTOs;
using System.Security.Claims;
using StackExchange.Redis;

namespace DogTeams.Api.Endpoints;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtTokenService _tokenService;
    private readonly IConnectionMultiplexer _redis;

    public AuthController(IUserService userService, IJwtTokenService tokenService, IConnectionMultiplexer redis)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return BadRequest("Password must be at least 6 characters.");
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        try
        {
            var user = await _userService.CreateUserAsync(request.Email, request.Password, request.Name);
            
            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.TeamId ?? "default", user.OwnerId);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Store refresh token in Redis with 7-day expiration
            var db = _redis.GetDatabase();
            await db.StringSetAsync($"refresh:{user.Id}", refreshToken, TimeSpan.FromDays(7));

            var response = new AuthTokenResponse(accessToken, refreshToken, 15 * 60);
            return Created("/api/auth/me", response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required.");

        var isValid = await _userService.VerifyPasswordAsync(request.Email, request.Password);
        if (!isValid)
            return Unauthorized("Invalid email or password.");

        var user = await _userService.GetUserByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized();

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.TeamId ?? "default", user.OwnerId);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token in Redis with 7-day expiration
        var db = _redis.GetDatabase();
        await db.StringSetAsync($"refresh:{user.Id}", refreshToken, TimeSpan.FromDays(7));

        var response = new AuthTokenResponse(accessToken, refreshToken, 15 * 60);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest("Refresh token is required.");

        // Extract user ID from refresh token in Redis
        var db = _redis.GetDatabase();
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        
        string? userId = null;
        foreach (var key in server.Keys(pattern: "refresh:*"))
        {
            var storedToken = await db.StringGetAsync(key.ToString());
            if (storedToken == request.RefreshToken)
            {
                userId = key.ToString().Replace("refresh:", "");
                break;
            }
        }

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Invalid refresh token.");

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        // Generate new access token
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.TeamId ?? "default", user.OwnerId);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Update refresh token in Redis
        await db.StringSetAsync($"refresh:{user.Id}", newRefreshToken, TimeSpan.FromDays(7));

        var response = new AuthTokenResponse(accessToken, newRefreshToken, 15 * 60);
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Delete refresh token from Redis
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync($"refresh:{userId}");

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var teamId = User.FindFirst("teamId")?.Value;
        var ownerId = User.FindFirst("ownerId")?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "user";

        var response = new AuthUserResponse(user.Id, user.Email, user.Name, teamId, ownerId, role);
        return Ok(response);
    }
}
