using System.Text.Json.Serialization;

namespace DogTeams.Api.Models;

/// <summary>
/// Represents a NAFA-registered club that fields one or more racing Teams.
/// Stored in the Cosmos DB "clubs" container, partition key: /clubId.
/// </summary>
public class Club
{
    /// <summary>Internal unique identifier (Guid-based string).</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Cosmos DB partition key — mirrors Id for this container.</summary>
    [JsonPropertyName("clubId")]
    public string ClubId { get; set; } = string.Empty;

    /// <summary>Cosmos DB type discriminator.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "club";

    /// <summary>NAFA-assigned club number. Nullable until officially registered with NAFA.</summary>
    [JsonPropertyName("nafaClubNumber")]
    public string? NafaClubNumber { get; set; }

    /// <summary>Club display name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Geographic region (e.g., "Northeast", "Pacific").</summary>
    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    /// <summary>Home region for NAFA regional championship qualification.</summary>
    [JsonPropertyName("homeRegion")]
    public string HomeRegion { get; set; } = string.Empty;

    /// <summary>UTC timestamp when this record was created.</summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when this record was last updated.</summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
