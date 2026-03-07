using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DogTeams.Api.Models;
using FluentAssertions;

namespace DogTeams.Tests.Integration.Api;

/// <summary>
/// Integration tests for Dogs API endpoints.
/// Tests owner/team-scoped dog CRUD operations and hierarchical data access.
/// </summary>
public class DogsApiIntegrationTests : IClassFixture<AppHostFixture>
{
    private readonly HttpClient _client;

    public DogsApiIntegrationTests(AppHostFixture fixture)
    {
        _client = fixture.ApiClient;
    }

    private async Task<(string Token, Team Team, Owner Owner)> SetupAuthenticatedUserWithTeamAndOwnerAsync()
    {
        var uniqueEmail = $"dog-{Guid.NewGuid()}@example.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = uniqueEmail,
            Password = "TestPassword123!",
            Name = "Dogs Test User"
        });

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = uniqueEmail,
            Password = "TestPassword123!"
        });

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var token = loginBody!.AccessToken;
        var authenticatedClient = CreateAuthenticatedClient(token);

        // Create a team
        var teamResponse = await authenticatedClient.PostAsJsonAsync("/api/teams", new
        {
            Name = $"Dogs Team {Guid.NewGuid()}",
            Description = "Team for dogs testing"
        });
        var team = await teamResponse.Content.ReadFromJsonAsync<Team>();

        // Create an owner
        var ownerResponse = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team!.Id}/owners", new
        {
            Name = $"Dogs Owner {Guid.NewGuid()}",
            Email = $"owner-{Guid.NewGuid()}@example.com"
        });
        var owner = await ownerResponse.Content.ReadFromJsonAsync<Owner>();

        return (token, team, owner!);
    }

    private HttpClient CreateAuthenticatedClient(string token)
    {
        var client = new HttpClient { BaseAddress = _client.BaseAddress };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task GetDogsByTeamId_WithValidTeamId_ReturnsDogs()
    {
        var (token, team, _) = await SetupAuthenticatedUserWithTeamAndOwnerAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        var response = await authenticatedClient.GetAsync($"/api/teams/{team.Id}/dogs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dogs = await response.Content.ReadFromJsonAsync<List<Dog>>();
        dogs.Should().NotBeNull();
        dogs.Should().BeEmpty();  // New team should have no dogs
    }

    [Fact]
    public async Task PostDog_CreatesDogInTeamUnderOwner()
    {
        var (token, team, owner) = await SetupAuthenticatedUserWithTeamAndOwnerAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        var dogDto = new
        {
            Name = $"Dog {Guid.NewGuid()}",
            DateOfBirth = DateTime.UtcNow.AddYears(-3)
        };

        var response = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team.Id}/owners/{owner.Id}/dogs", dogDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdDog = await response.Content.ReadFromJsonAsync<Dog>();
        createdDog.Should().NotBeNull();
        createdDog!.Name.Should().Be(dogDto.Name);
        createdDog.TeamId.Should().Be(team.Id);
        createdDog.OwnerId.Should().Be(owner.Id);
    }

    [Fact]
    public async Task PostDog_WithoutAuthentication_Returns401()
    {
        var client = new HttpClient { BaseAddress = _client.BaseAddress };
        var dogDto = new { Name = "Unauthorized Dog", DateOfBirth = DateTime.UtcNow.AddYears(-2) };

        var response = await client.PostAsJsonAsync($"/api/teams/some-team-id/owners/some-owner-id/dogs", dogDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDogsByOwnerId_ReturnsDogsBelongingToOwner()
    {
        var (token, team, owner) = await SetupAuthenticatedUserWithTeamAndOwnerAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        // Create dogs for the owner
        var dog1Response = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team.Id}/owners/{owner.Id}/dogs", new
        {
            Name = $"Owner Dog 1 {Guid.NewGuid()}",
            DateOfBirth = DateTime.UtcNow.AddYears(-2)
        });
        var dog1 = await dog1Response.Content.ReadFromJsonAsync<Dog>();

        var dog2Response = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team.Id}/owners/{owner.Id}/dogs", new
        {
            Name = $"Owner Dog 2 {Guid.NewGuid()}",
            DateOfBirth = DateTime.UtcNow.AddYears(-1)
        });
        var dog2 = await dog2Response.Content.ReadFromJsonAsync<Dog>();

        // Get all dogs for owner
        var allDogsResponse = await authenticatedClient.GetAsync($"/api/teams/{team.Id}/owners/{owner.Id}/dogs");
        var allDogs = await allDogsResponse.Content.ReadFromJsonAsync<List<Dog>>();

        allDogs.Should().NotBeNull();
        allDogs!.Count.Should().BeGreaterThanOrEqualTo(2);
        allDogs.Should().Contain(d => d.Id == dog1!.Id);
        allDogs.Should().Contain(d => d.Id == dog2!.Id);
        allDogs.All(d => d.OwnerId == owner.Id).Should().BeTrue();
    }

    [Fact]
    public async Task GetDogById_ReturnsDogInTeamUnderOwner()
    {
        var (token, team, owner) = await SetupAuthenticatedUserWithTeamAndOwnerAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        // Create a dog
        var createResponse = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team.Id}/owners/{owner.Id}/dogs", new
        {
            Name = $"Get Dog {Guid.NewGuid()}",
            DateOfBirth = DateTime.UtcNow.AddYears(-4)
        });
        var createdDog = await createResponse.Content.ReadFromJsonAsync<Dog>();
        createdDog.Should().NotBeNull();

        // Get the dog
        var getResponse = await authenticatedClient.GetAsync($"/api/teams/{team.Id}/dogs/{createdDog!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var retrievedDog = await getResponse.Content.ReadFromJsonAsync<Dog>();
        retrievedDog.Should().NotBeNull();
        retrievedDog!.Id.Should().Be(createdDog.Id);
        retrievedDog.TeamId.Should().Be(team.Id);
        retrievedDog.OwnerId.Should().Be(owner.Id);
    }

    [Fact]
    public async Task GetDogById_WithNonexistentId_Returns404()
    {
        var (token, team, _) = await SetupAuthenticatedUserWithTeamAndOwnerAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        var response = await authenticatedClient.GetAsync($"/api/teams/{team.Id}/dogs/nonexistent-{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutDog_UpdatesDog()
    {
        var (token, team, owner) = await SetupAuthenticatedUserWithTeamAndOwnerAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        // Create a dog
        var createResponse = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team.Id}/owners/{owner.Id}/dogs", new
        {
            Name = $"Update Dog {Guid.NewGuid()}",
            DateOfBirth = DateTime.UtcNow.AddYears(-5)
        });
        var createdDog = await createResponse.Content.ReadFromJsonAsync<Dog>();
        createdDog.Should().NotBeNull();

        // Update the dog
        var updatedDog = new Dog
        {
            Id = createdDog!.Id,
            TeamId = team.Id,
            OwnerId = owner.Id,
            Name = "Updated Dog Name",
            DateOfBirth = createdDog.DateOfBirth,
            CreatedAt = createdDog.CreatedAt
        };

        var updateResponse = await authenticatedClient.PutAsJsonAsync($"/api/teams/{team.Id}/dogs/{createdDog.Id}", updatedDog);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await updateResponse.Content.ReadFromJsonAsync<Dog>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Dog Name");
    }

    [Fact]
    public async Task DeleteDog_DeletesDogFromTeam()
    {
        var (token, team, owner) = await SetupAuthenticatedUserWithTeamAndOwnerAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        // Create a dog
        var createResponse = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team.Id}/owners/{owner.Id}/dogs", new
        {
            Name = $"Delete Dog {Guid.NewGuid()}",
            DateOfBirth = DateTime.UtcNow.AddYears(-2)
        });
        var createdDog = await createResponse.Content.ReadFromJsonAsync<Dog>();
        createdDog.Should().NotBeNull();

        // Delete the dog
        var deleteResponse = await authenticatedClient.DeleteAsync($"/api/teams/{team.Id}/dogs/{createdDog!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await authenticatedClient.GetAsync($"/api/teams/{team.Id}/dogs/{createdDog.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MultipleDogs_CanBeCreatedAndRetrieved()
    {
        var (token, team, owner) = await SetupAuthenticatedUserWithTeamAndOwnerAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        // Create multiple dogs
        var dog1Response = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team.Id}/owners/{owner.Id}/dogs", new
        {
            Name = $"Multi Dog 1 {Guid.NewGuid()}",
            DateOfBirth = DateTime.UtcNow.AddYears(-3)
        });
        var dog1 = await dog1Response.Content.ReadFromJsonAsync<Dog>();

        var dog2Response = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team.Id}/owners/{owner.Id}/dogs", new
        {
            Name = $"Multi Dog 2 {Guid.NewGuid()}",
            DateOfBirth = DateTime.UtcNow.AddYears(-2)
        });
        var dog2 = await dog2Response.Content.ReadFromJsonAsync<Dog>();

        // Get all dogs for team
        var allDogsResponse = await authenticatedClient.GetAsync($"/api/teams/{team.Id}/dogs");
        var allDogs = await allDogsResponse.Content.ReadFromJsonAsync<List<Dog>>();

        allDogs.Should().NotBeNull();
        allDogs!.Count.Should().BeGreaterThanOrEqualTo(2);
        allDogs.Should().Contain(d => d.Id == dog1!.Id);
        allDogs.Should().Contain(d => d.Id == dog2!.Id);
    }

    [Fact]
    public async Task TeamDataIsolation_DogsFromOtherTeamsNotAccessible()
    {
        var (token, team1, owner1) = await SetupAuthenticatedUserWithTeamAndOwnerAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        // Create another team and owner
        var team2Response = await authenticatedClient.PostAsJsonAsync("/api/teams", new
        {
            Name = $"Isolation Team {Guid.NewGuid()}",
            Description = "Second team"
        });
        var team2 = await team2Response.Content.ReadFromJsonAsync<Team>();

        var owner2Response = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team2!.Id}/owners", new
        {
            Name = $"Isolation Owner {Guid.NewGuid()}",
            Email = $"isolation-{Guid.NewGuid()}@example.com"
        });
        var owner2 = await owner2Response.Content.ReadFromJsonAsync<Owner>();

        // Dogs in team1 should not appear when querying team2
        var team1DogsResponse = await authenticatedClient.GetAsync($"/api/teams/{team1.Id}/dogs");
        var team1Dogs = await team1DogsResponse.Content.ReadFromJsonAsync<List<Dog>>();

        var team2DogsResponse = await authenticatedClient.GetAsync($"/api/teams/{team2.Id}/dogs");
        var team2Dogs = await team2DogsResponse.Content.ReadFromJsonAsync<List<Dog>>();

        team1Dogs.Should().NotBeNull();
        team2Dogs.Should().NotBeNull();
        // Both should be empty initially
        team1Dogs!.Should().BeEmpty();
        team2Dogs!.Should().BeEmpty();
    }

    private record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn);
}
