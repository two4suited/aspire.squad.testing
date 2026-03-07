namespace DogTeams.Api.Models;

/// <summary>Measurement classification for a dog's withers height.</summary>
public enum MeasurementType
{
    /// <summary>No measurement recorded.</summary>
    None,
    /// <summary>Temporary measurement valid during 15–24 months of age.</summary>
    Temporary,
    /// <summary>Permanent measurement with two matching OIR results.</summary>
    Permanent
}
