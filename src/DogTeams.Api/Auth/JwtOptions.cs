namespace DogTeams.Api.Auth;

/// <summary>
/// Configuration options for JWT token generation and validation.
/// </summary>
public class JwtOptions
{
    /// <summary>JWT signing key (should be at least 32 characters for HS256).</summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>JWT issuer claim value.</summary>
    public string Issuer { get; set; } = "DogTeams";

    /// <summary>JWT audience claim value.</summary>
    public string Audience { get; set; } = "DogTeamsUsers";

    /// <summary>Access token expiration in minutes.</summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>Refresh token expiration in days.</summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
