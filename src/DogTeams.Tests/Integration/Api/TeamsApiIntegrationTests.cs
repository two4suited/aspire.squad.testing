using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DogTeams.Api.Models;
using FluentAssertions;

namespace DogTeams.Tests.Integration.Api;

/// <summary>
/// Integration tests for Teams API endpoints.
/// Tests the full API flow including authentication and CRUD operations.
/// </summary>
public class TeamsApiIntegrationTests : IClassFixture<AppHostFixture>
{
    private readonly HttpClient _client;

    public TeamsApiIntegrationTests(AppHostFixture fixture)
    {
        _client = fixture.ApiClient;
    }

    private async Task<string> AuthenticateAndGetTokenAsync()
    {
        var uniqueEmail = $"teams-{Guid.NewGuid()}@example.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = uniqueEmail,
            Password = "TestPassword123!",
            Name = "Teams Test User"
        });

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = uniqueEmail,
            Password = "TestPassword123!"
        });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginBody.Should().NotBeNull();
        return loginBody!.AccessToken;
    }

    private HttpClient CreateAuthenticatedClient(string token)
    {
        var client = new HttpClient { BaseAddress = _client.BaseAddress };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task GetTeams_WithoutAuthentication_Returns401()
    {
        var client = new HttpClient { BaseAddress = _client.BaseAddress };
        var response = await client.GetAsync("/api/teams");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTeams_WithAuthentication_Returns200()
    {
        var token = await AuthenticateAndGetTokenAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        var response = await authenticatedClient.GetAsync("/api/teams");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostTeam_CreatesNewTeam()
    {
        var token = await AuthenticateAndGetTokenAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        var teamDto = new
        {
            Name = $"Team {Guid.NewGuid()}",
            Description = "Integration Test Team"
        };

        var response = await authenticatedClient.PostAsJsonAsync("/api/teams", teamDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var team = await response.Content.ReadFromJsonAsync<Team>();
        team.Should().NotBeNull();
        team!.Name.Should().Be(teamDto.Name);
        team.Description.Should().Be(teamDto.Description);
    }

    [Fact]
    public async Task PostTeam_WithoutAuthentication_Returns401()
    {
        var client = new HttpClient { BaseAddress = _client.BaseAddress };
        var teamDto = new { Name = "Unauthenticated Team", Description = "Should Fail" };

        var response = await client.PostAsJsonAsync("/api/teams", teamDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTeamById_ReturnsTeam()
    {
        var token = await AuthenticateAndGetTokenAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        // Create a team
        var createResponse = await authenticatedClient.PostAsJsonAsync("/api/teams", new
        {
            Name = $"Get Team {Guid.NewGuid()}",
            Description = "To be retrieved"
        });
        var createdTeam = await createResponse.Content.ReadFromJsonAsync<Team>();
        createdTeam.Should().NotBeNull();

        // Get the team
        var getResponse = await authenticatedClient.GetAsync($"/api/teams/{createdTeam!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var retrievedTeam = await getResponse.Content.ReadFromJsonAsync<Team>();
        retrievedTeam.Should().NotBeNull();
        retrievedTeam!.Id.Should().Be(createdTeam.Id);
        retrievedTeam.Name.Should().Be(createdTeam.Name);
    }

    [Fact]
    public async Task GetTeamById_WithNonexistentId_Returns404()
    {
        var token = await AuthenticateAndGetTokenAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        var response = await authenticatedClient.GetAsync($"/api/teams/nonexistent-{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutTeam_UpdatesTeam()
    {
        var token = await AuthenticateAndGetTokenAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        // Create a team
        var createResponse = await authenticatedClient.PostAsJsonAsync("/api/teams", new
        {
            Name = $"Update Test {Guid.NewGuid()}",
            Description = "Original description"
        });
        var createdTeam = await createResponse.Content.ReadFromJsonAsync<Team>();
        createdTeam.Should().NotBeNull();

        // Update the team
        var updatedTeam = new Team
        {
            Id = createdTeam!.Id,
            Name = "Updated Team Name",
            Description = "Updated description",
            CreatedAt = createdTeam.CreatedAt
        };

        var updateResponse = await authenticatedClient.PutAsJsonAsync($"/api/teams/{createdTeam.Id}", updatedTeam);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await updateResponse.Content.ReadFromJsonAsync<Team>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Team Name");
        result.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task DeleteTeam_DeletesTeam()
    {
        var token = await AuthenticateAndGetTokenAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        // Create a team
        var createResponse = await authenticatedClient.PostAsJsonAsync("/api/teams", new
        {
            Name = $"Delete Test {Guid.NewGuid()}",
            Description = "To be deleted"
        });
        var createdTeam = await createResponse.Content.ReadFromJsonAsync<Team>();
        createdTeam.Should().NotBeNull();

        // Delete the team
        var deleteResponse = await authenticatedClient.DeleteAsync($"/api/teams/{createdTeam!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await authenticatedClient.GetAsync($"/api/teams/{createdTeam.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTeam_WithoutAuthentication_Returns401()
    {
        var client = new HttpClient { BaseAddress = _client.BaseAddress };
        var response = await client.DeleteAsync($"/api/teams/some-id");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MultipleTeams_CanBeCreatedAndRetrieved()
    {
        var token = await AuthenticateAndGetTokenAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        // Create multiple teams
        var team1Response = await authenticatedClient.PostAsJsonAsync("/api/teams", new
        {
            Name = $"Multi Team 1 {Guid.NewGuid()}",
            Description = "First team"
        });
        var team1 = await team1Response.Content.ReadFromJsonAsync<Team>();

        var team2Response = await authenticatedClient.PostAsJsonAsync("/api/teams", new
        {
            Name = $"Multi Team 2 {Guid.NewGuid()}",
            Description = "Second team"
        });
        var team2 = await team2Response.Content.ReadFromJsonAsync<Team>();

        // Get all teams
        var allTeamsResponse = await authenticatedClient.GetAsync("/api/teams");
        var allTeams = await allTeamsResponse.Content.ReadFromJsonAsync<List<Team>>();

        allTeams.Should().NotBeNull();
        allTeams!.Count.Should().BeGreaterThanOrEqualTo(2);
        allTeams.Should().Contain(t => t.Id == team1!.Id);
        allTeams.Should().Contain(t => t.Id == team2!.Id);
    }

    private record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn);
}
