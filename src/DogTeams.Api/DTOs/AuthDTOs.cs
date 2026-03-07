namespace DogTeams.Api.DTOs;

/// <summary>Request to register a new user.</summary>
public record RegisterRequest(
    string Email,
    string Password,
    string Name
);

/// <summary>Request to log in with email and password.</summary>
public record LoginRequest(
    string Email,
    string Password
);

/// <summary>Request to refresh an expired access token.</summary>
public record RefreshTokenRequest(
    string RefreshToken
);

/// <summary>Response containing JWT tokens after successful authentication.</summary>
public record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType = "Bearer"
);

/// <summary>Response containing authenticated user profile information.</summary>
public record AuthUserResponse(
    string UserId,
    string Email,
    string Name,
    string? TeamId,
    string? OwnerId,
    string Role
);
