using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DogTeams.Api.Models;
using FluentAssertions;

namespace DogTeams.Tests.Integration.Api;

/// <summary>
/// Integration tests for Owners API endpoints.
/// Tests team-scoped owner CRUD operations and data isolation.
/// </summary>
public class OwnersApiIntegrationTests : IClassFixture<AppHostFixture>
{
    private readonly HttpClient _client;

    public OwnersApiIntegrationTests(AppHostFixture fixture)
    {
        _client = fixture.ApiClient;
    }

    private async Task<(string Token, Team Team)> SetupAuthenticatedUserWithTeamAsync()
    {
        var uniqueEmail = $"owner-{Guid.NewGuid()}@example.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = uniqueEmail,
            Password = "TestPassword123!",
            Name = "Owners Test User"
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
            Name = $"Owners Team {Guid.NewGuid()}",
            Description = "Team for owners testing"
        });

        var team = await teamResponse.Content.ReadFromJsonAsync<Team>();
        return (token, team!);
    }

    private HttpClient CreateAuthenticatedClient(string token)
    {
        var client = new HttpClient { BaseAddress = _client.BaseAddress };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task GetOwnersByTeamId_WithValidTeamId_ReturnsOwners()
    {
        var (token, team) = await SetupAuthenticatedUserWithTeamAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        var response = await authenticatedClient.GetAsync($"/api/teams/{team.Id}/owners");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var owners = await response.Content.ReadFromJsonAsync<List<Owner>>();
        owners.Should().NotBeNull();
        owners.Should().BeEmpty();  // New team should have no owners
    }

    [Fact]
    public async Task PostOwner_CreatesOwnerInTeam()
    {
        var (token, team) = await SetupAuthenticatedUserWithTeamAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        var ownerDto = new
        {
            Name = $"Owner {Guid.NewGuid()}",
            Email = $"owner-{Guid.NewGuid()}@example.com"
        };

        var response = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team.Id}/owners", ownerDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdOwner = await response.Content.ReadFromJsonAsync<Owner>();
        createdOwner.Should().NotBeNull();
        createdOwner!.Name.Should().Be(ownerDto.Name);
        createdOwner.TeamId.Should().Be(team.Id);
    }

    [Fact]
    public async Task PostOwner_WithoutAuthentication_Returns401()
    {
        var client = new HttpClient { BaseAddress = _client.BaseAddress };
        var ownerDto = new { Name = "Unauthorized Owner", Email = "test@example.com" };

        var response = await client.PostAsJsonAsync($"/api/teams/some-team-id/owners", ownerDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOwnerById_ReturnsOwnerInTeam()
    {
        var (token, team) = await SetupAuthenticatedUserWithTeamAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        // Create an owner
        var createResponse = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team.Id}/owners", new
        {
            Name = $"Get Owner {Guid.NewGuid()}",
            Email = $"get-{Guid.NewGuid()}@example.com"
        });
        var createdOwner = await createResponse.Content.ReadFromJsonAsync<Owner>();
        createdOwner.Should().NotBeNull();

        // Get the owner
        var getResponse = await authenticatedClient.GetAsync($"/api/teams/{team.Id}/owners/{createdOwner!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var retrievedOwner = await getResponse.Content.ReadFromJsonAsync<Owner>();
        retrievedOwner.Should().NotBeNull();
        retrievedOwner!.Id.Should().Be(createdOwner.Id);
        retrievedOwner.TeamId.Should().Be(team.Id);
    }

    [Fact]
    public async Task MultipleOwners_CanBeCreatedAndRetrieved()
    {
        var (token, team) = await SetupAuthenticatedUserWithTeamAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        // Create multiple owners
        var owner1Response = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team.Id}/owners", new
        {
            Name = $"Multi Owner 1 {Guid.NewGuid()}",
            Email = $"multi1-{Guid.NewGuid()}@example.com"
        });
        var owner1 = await owner1Response.Content.ReadFromJsonAsync<Owner>();

        var owner2Response = await authenticatedClient.PostAsJsonAsync($"/api/teams/{team.Id}/owners", new
        {
            Name = $"Multi Owner 2 {Guid.NewGuid()}",
            Email = $"multi2-{Guid.NewGuid()}@example.com"
        });
        var owner2 = await owner2Response.Content.ReadFromJsonAsync<Owner>();

        // Get all owners for team
        var allOwnersResponse = await authenticatedClient.GetAsync($"/api/teams/{team.Id}/owners");
        var allOwners = await allOwnersResponse.Content.ReadFromJsonAsync<List<Owner>>();

        allOwners.Should().NotBeNull();
        allOwners!.Count.Should().BeGreaterThanOrEqualTo(2);
        allOwners.Should().Contain(o => o.Id == owner1!.Id);
        allOwners.Should().Contain(o => o.Id == owner2!.Id);
    }

    private record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn);
}
