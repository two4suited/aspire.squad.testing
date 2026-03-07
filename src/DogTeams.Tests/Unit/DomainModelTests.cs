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
