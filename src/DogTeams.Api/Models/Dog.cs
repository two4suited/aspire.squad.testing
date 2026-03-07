using System.Text.Json.Serialization;

namespace DogTeams.Api.Models;

/// <summary>
/// Represents a dog competing in flyball, owned by an Owner and belonging to a Team.
/// Stored in the Cosmos DB "dogs" container, partition key: /teamId.
/// </summary>
public class Dog
{
    /// <summary>Internal unique identifier (Guid-based string).</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Cosmos DB type discriminator.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "dog";

    /// <summary>ID of the Owner who handles this dog.</summary>
    [JsonPropertyName("ownerId")]
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>Denormalized team ID for partition-efficient queries.</summary>
    [JsonPropertyName("teamId")]
    public string TeamId { get; set; } = string.Empty;

    /// <summary>Dog's call name or registered name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Legacy free-text breed field. Kept for backwards compatibility.
    /// Prefer <see cref="BreedId"/> for new records.
    /// </summary>
    [Obsolete("Use BreedId to reference the Breed entity. This field is retained for backwards compatibility only.")]
    [JsonPropertyName("breed")]
    public string Breed { get; set; } = string.Empty;

    /// <summary>Reference to the Breed entity ID. Nullable for records created before breed normalization.</summary>
    [JsonPropertyName("breedId")]
    public string? BreedId { get; set; }

    /// <summary>Dog's date of birth. Used for age eligibility and measurement expiry calculations.</summary>
    [JsonPropertyName("dateOfBirth")]
    public DateTime DateOfBirth { get; set; }

    // ─── NAFA Registration ────────────────────────────────────────────────────

    /// <summary>NAFA-assigned Competition Racing Number. Nullable until registered with NAFA. Immutable once set.</summary>
    [JsonPropertyName("nafaCrn")]
    public string? NafaCrn { get; set; }

    // ─── Jump Height Measurement ───────────────────────────────────────────────

    /// <summary>Measured withers height in inches.</summary>
    [JsonPropertyName("withersHeightInches")]
    public decimal? WithersHeightInches { get; set; }

    /// <summary>
    /// Calculated jump height in inches. Formula: withers - 6", minimum 7", maximum 14", rounded down to whole inch.
    /// </summary>
    [JsonPropertyName("jumpHeightInches")]
    public decimal? JumpHeightInches { get; set; }

    /// <summary>Classification of the current height measurement.</summary>
    [JsonPropertyName("measurementType")]
    public MeasurementType MeasurementType { get; set; } = MeasurementType.None;

    /// <summary>Date the height measurement was taken.</summary>
    [JsonPropertyName("measurementDate")]
    public DateTime? MeasurementDate { get; set; }

    /// <summary>
    /// Expiry date for a Temporary measurement.
    /// Temporary measurements are valid during 15–24 months of age; null for Permanent measurements.
    /// </summary>
    [JsonPropertyName("measurementExpiresAt")]
    public DateTime? MeasurementExpiresAt { get; set; }

    // ─── Points (materialized, updated on heat result entry) ──────────────────

    /// <summary>Materialized lifetime regular-class points. Updated on each heat result entry.</summary>
    [JsonPropertyName("lifetimePoints")]
    public int LifetimePoints { get; set; } = 0;

    /// <summary>Materialized lifetime multibreed-class points. Updated on each heat result entry.</summary>
    [JsonPropertyName("multibreedLifetimePoints")]
    public int MultibreedLifetimePoints { get; set; } = 0;

    // ─── Timestamps ───────────────────────────────────────────────────────────

    /// <summary>UTC timestamp when this record was created.</summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
