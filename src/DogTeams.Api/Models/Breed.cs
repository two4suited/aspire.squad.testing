using System.Text.Json.Serialization;

namespace DogTeams.Api.Models;

/// <summary>
/// Represents a dog breed from the AKC breed list.
/// Stored in the Cosmos DB "breeds" container, partition key: /id.
/// </summary>
public class Breed
{
    /// <summary>Internal unique identifier (Guid-based string).</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Cosmos DB type discriminator.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "breed";

    /// <summary>Breed name (e.g., "Border Collie").</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>AKC breed code, if applicable.</summary>
    [JsonPropertyName("akcCode")]
    public string? AkcCode { get; set; }

    /// <summary>Whether this breed is active/selectable in the system.</summary>
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    /// <summary>UTC timestamp when this record was created.</summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
