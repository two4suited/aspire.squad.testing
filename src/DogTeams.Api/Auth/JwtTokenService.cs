using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace DogTeams.Api.Auth;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>Generates an access JWT token with the given claims.</summary>
    string GenerateAccessToken(string userId, string teamId, string? ownerId = null, string role = "user");

    /// <summary>Generates a refresh token (random string for Redis storage).</summary>
    string GenerateRefreshToken();

    /// <summary>Validates a JWT token and returns the principal if valid.</summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>Extracts the user ID from a JWT token.</summary>
    string? GetUserIdFromToken(string token);
}

/// <summary>
/// JWT token service implementation using System.IdentityModel.Tokens.Jwt.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly TokenValidationParameters _validationParameters;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrEmpty(_options.SigningKey))
            throw new InvalidOperationException("JWT SigningKey must be configured.");

        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    public string GenerateAccessToken(string userId, string teamId, string? ownerId = null, string role = "user")
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        if (string.IsNullOrEmpty(teamId))
            throw new ArgumentException("Team ID cannot be null or empty.", nameof(teamId));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),  // sub
            new Claim("teamId", teamId),
            new Claim(ClaimTypes.Role, role)
        };

        if (!string.IsNullOrEmpty(ownerId))
            claims.Add(new Claim("ownerId", ownerId));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, _validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public string? GetUserIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            return jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        catch
        {
            return null;
        }
    }
}
