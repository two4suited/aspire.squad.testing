using DogTeams.Api.Models;
using FluentAssertions;

namespace DogTeams.Tests.Unit;

public class DomainModelTests
{
    [Fact]
    public void Dog_DefaultConstruction_HasNewGuidStringId()
    {
        var dog = new Dog();
        dog.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(dog.Id, out _).Should().BeTrue();
    }

    [Fact]
    public void Dog_DefaultConstruction_HasEmptyStringFields()
    {
        var dog = new Dog();
        dog.Name.Should().BeEmpty();
        dog.Breed.Should().BeEmpty();
        dog.OwnerId.Should().BeEmpty();
        dog.TeamId.Should().BeEmpty();
    }

    [Fact]
    public void Dog_DefaultConstruction_HasCreatedAtSet()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var dog = new Dog();
        dog.CreatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Dog_CanSetNameBreedAndOwnership()
    {
        var ownerId = Guid.NewGuid().ToString();
        var teamId = Guid.NewGuid().ToString();
        var dog = new Dog { Name = "Rex", Breed = "German Shepherd", OwnerId = ownerId, TeamId = teamId };

        dog.Name.Should().Be("Rex");
        dog.Breed.Should().Be("German Shepherd");
        dog.OwnerId.Should().Be(ownerId);
        dog.TeamId.Should().Be(teamId);
    }

    [Fact]
    public void Owner_DefaultConstruction_HasNewGuidStringId()
    {
        var owner = new Owner();
        owner.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(owner.Id, out _).Should().BeTrue();
    }

    [Fact]
    public void Owner_DefaultConstruction_HasEmptyDogsCollection()
    {
        var owner = new Owner();
        owner.Dogs.Should().NotBeNull();
        owner.Dogs.Should().BeEmpty();
    }

    [Fact]
    public void Owner_DefaultConstruction_HasEmptyStringFields()
    {
        var owner = new Owner();
        owner.Name.Should().BeEmpty();
        owner.Email.Should().BeEmpty();
        owner.TeamId.Should().BeEmpty();
        owner.UserId.Should().BeEmpty();
    }

    [Fact]
    public void Owner_DefaultConstruction_HasCreatedAtSet()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var owner = new Owner();
        owner.CreatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Owner_CanAddDogs_RelationshipIsOneToMany()
    {
        var owner = new Owner { Name = "Alice", Email = "alice@example.com" };
        var dog1 = new Dog { Name = "Buddy", Breed = "Labrador", OwnerId = owner.Id };
        var dog2 = new Dog { Name = "Charlie", Breed = "Poodle", OwnerId = owner.Id };

        owner.Dogs.Add(dog1);
        owner.Dogs.Add(dog2);

        owner.Dogs.Should().HaveCount(2);
        owner.Dogs.Should().AllSatisfy(d => d.OwnerId.Should().Be(owner.Id));
    }

    [Fact]
    public void Team_DefaultConstruction_HasNewGuidStringId()
    {
        var team = new Team();
        team.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(team.Id, out _).Should().BeTrue();
    }

    [Fact]
    public void Team_DefaultConstruction_HasEmptyStringFields()
    {
        var team = new Team();
        team.Name.Should().BeEmpty();
        team.Description.Should().BeEmpty();
    }

    [Fact]
    public void Team_DefaultConstruction_HasCreatedAtSet()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var team = new Team();
        team.CreatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Team_CanSetNameAndDescription()
    {
        var team = new Team { Name = "Alpha Pack", Description = "The first team" };
        team.Name.Should().Be("Alpha Pack");
        team.Description.Should().Be("The first team");
    }

    [Fact]
    public void DomainRelationshipChain_OwnerHasDogs_DogCarriesTeamId()
    {
        // Validates: User(auth) → Owner → Team; Owner → [Dog, Dog, ...]
        // Dog carries TeamId as a denormalized field for Cosmos DB partition key queries.
        var team = new Team { Name = "Bravo Pack" };
        var owner = new Owner { Name = "Bob", TeamId = team.Id };
        var dog1 = new Dog { Name = "Max", OwnerId = owner.Id, TeamId = owner.TeamId };
        var dog2 = new Dog { Name = "Bella", OwnerId = owner.Id, TeamId = owner.TeamId };

        owner.Dogs.Add(dog1);
        owner.Dogs.Add(dog2);

        owner.TeamId.Should().Be(team.Id);
        owner.Dogs.Should().HaveCount(2);
        owner.Dogs.Should().AllSatisfy(d =>
        {
            d.OwnerId.Should().Be(owner.Id);
            d.TeamId.Should().Be(team.Id);
        });
    }
}

// ─── FB-1: Club, Breed, and extended Dog tests ───────────────────────────────
// Wrapped in #if false until Backend lands Club, Breed, and Dog flyball fields.
// Remove #if false / #endif when DogTeams.Api.Models contains these types.

#if false

public class ClubModelTests
{
    [Fact]
    public void Club_DefaultValues_AreCorrect()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var club = new Club();

        club.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(club.Id, out _).Should().BeTrue();
        club.Type.Should().Be("club");
        club.CreatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Club_NafaClubNumber_IsNullable()
    {
        var club = new Club();
        club.NafaClubNumber.Should().BeNull();
    }
}

public class DogFlyballFieldTests
{
    [Fact]
    public void Dog_FlyballDefaults_AreCorrect()
    {
        var dog = new Dog();

        dog.LifetimePoints.Should().Be(0);
        dog.MultibreedLifetimePoints.Should().Be(0);
        dog.MeasurementType.Should().Be(MeasurementType.None);
    }

    [Fact]
    public void Dog_NafaCrn_IsNullableByDefault()
    {
        var dog = new Dog();
        dog.NafaCrn.Should().BeNull();
    }

    [Fact]
    public void Dog_WithersHeight_IsNullableByDefault()
    {
        var dog = new Dog();
        dog.WithersHeightInches.Should().BeNull();
    }
}

public class BreedModelTests
{
    [Fact]
    public void Breed_DefaultValues_AreCorrect()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var breed = new Breed();

        breed.IsActive.Should().BeTrue();
        breed.Type.Should().Be("breed");
        breed.CreatedAt.Should().BeAfter(before);
    }
}

#endif

// ─── Jump height calculation — pure math, no Backend types required ───────────
// Rule: JumpHeightInches = floor(WithersHeightInches - 6), min 7, max 14.
// These tests encode the spec and should run immediately.
// TODO: When Backend implements JumpHeightCalculator (or equivalent), replace the
//       inline helper below with a call to the production method.

public class JumpHeightCalculationTests
{
    /// <summary>
    /// Encodes the NAFA jump height rule: floor(withers - 6), clamped to [7, 14].
    /// Replace with production method reference once Backend implements it.
    /// </summary>
    private static int CalculateJumpHeight(decimal withersHeightInches)
    {
        var jump = (int)Math.Floor(withersHeightInches - 6m);
        return Math.Clamp(jump, 7, 14);
    }

    [Fact]
    public void JumpHeight_ForDog_14Inch_WithersIs8Inches()
    {
        // withers=14 → floor(14-6)=8 → 8 ≥ min(7) → jump=8
        CalculateJumpHeight(14m).Should().Be(8);
    }

    [Fact]
    public void JumpHeight_ForDog_12Inch_Withers_Is6Inches()
    {
        // withers=12 → floor(12-6)=6 → below min(7) → jump=7
        CalculateJumpHeight(12m).Should().Be(7);
    }

    [Fact]
    public void JumpHeight_ForDog_20Inch_Withers_Is14Inches()
    {
        // withers=20 → floor(20-6)=14 → at max(14) → jump=14
        CalculateJumpHeight(20m).Should().Be(14);
    }

    [Fact]
    public void JumpHeight_ForDog_22Inch_Withers_Is14Inches()
    {
        // withers=22 → floor(22-6)=16 → exceeds max(14) → jump=14
        CalculateJumpHeight(22m).Should().Be(14);
    }

    [Fact]
    public void JumpHeight_RoundsDown()
    {
        // withers=13.75 → floor(13.75-6)=floor(7.75)=7 → jump=7
        CalculateJumpHeight(13.75m).Should().Be(7);
    }
}
