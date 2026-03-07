using DogTeams.Api.Models;

namespace DogTeams.Api.Data;

/// <summary>
/// Seed data for the Breed entity. Contains common flyball breeds from the AKC list.
/// Used as the initial data set until an admin-managed Cosmos collection is populated.
/// </summary>
public static class BreedSeedData
{
    private static readonly List<Breed> _breeds = new()
    {
        new Breed { Name = "Border Collie",                AkcCode = "BC",  IsActive = true },
        new Breed { Name = "Australian Shepherd",          AkcCode = "ASHE",IsActive = true },
        new Breed { Name = "Golden Retriever",             AkcCode = "GR",  IsActive = true },
        new Breed { Name = "Labrador Retriever",           AkcCode = "LR",  IsActive = true },
        new Breed { Name = "Jack Russell Terrier",         AkcCode = "JRT", IsActive = true },
        new Breed { Name = "Whippet",                      AkcCode = "WH",  IsActive = true },
        new Breed { Name = "Staffordshire Bull Terrier",   AkcCode = "SBT", IsActive = true },
        new Breed { Name = "Belgian Malinois",             AkcCode = "BM",  IsActive = true },
        new Breed { Name = "Shetland Sheepdog",            AkcCode = "SS",  IsActive = true },
        new Breed { Name = "Pembroke Welsh Corgi",         AkcCode = "PWC", IsActive = true },
        new Breed { Name = "Miniature American Shepherd",  AkcCode = "MAS", IsActive = true },
        new Breed { Name = "Australian Cattle Dog",        AkcCode = "ACD", IsActive = true },
        new Breed { Name = "Kooikerhondje",                AkcCode = "KOI", IsActive = true },
        new Breed { Name = "English Springer Spaniel",     AkcCode = "ESS", IsActive = true },
        new Breed { Name = "Border Terrier",               AkcCode = "BT",  IsActive = true },
        new Breed { Name = "Vizsla",                       AkcCode = "VIZ", IsActive = true },
        new Breed { Name = "Weimaraner",                   AkcCode = "WEI", IsActive = true },
        new Breed { Name = "Standard Poodle",              AkcCode = "SPO", IsActive = true },
        new Breed { Name = "Miniature Poodle",             AkcCode = "MPO", IsActive = true },
        new Breed { Name = "Mixed Breed",                  AkcCode = null,  IsActive = true },
        new Breed { Name = "Belgian Tervuren",             AkcCode = "BTV", IsActive = true },
        new Breed { Name = "Dutch Shepherd",               AkcCode = "DS",  IsActive = true },
        new Breed { Name = "Collie",                       AkcCode = "COL", IsActive = true },
        new Breed { Name = "Flat-Coated Retriever",        AkcCode = "FCR", IsActive = true },
        new Breed { Name = "Nova Scotia Duck Tolling Retriever", AkcCode = "NSDTR", IsActive = true },
        new Breed { Name = "Rat Terrier",                  AkcCode = "RT",  IsActive = true },
        new Breed { Name = "Parson Russell Terrier",       AkcCode = "PRT", IsActive = true },
        new Breed { Name = "Welsh Corgi (Cardigan)",       AkcCode = "CWC", IsActive = true },
        new Breed { Name = "Basenji",                      AkcCode = "BAS", IsActive = true },
        new Breed { Name = "Italian Greyhound",            AkcCode = "IG",  IsActive = true },
    };

    /// <summary>Returns the complete seeded breed list.</summary>
    public static IReadOnlyList<Breed> GetAll() => _breeds.AsReadOnly();
}
