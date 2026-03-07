using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

// NOTE: These tests require the full Aspire host (DogTeams.AppHost + DogTeams.Api).
// They are skipped until those projects are available and the AppHostFixture is wired up.

public class TeamsIntegrationTests : IClassFixture<AppHostFixture>
{
    private readonly HttpClient _client;

    public TeamsIntegrationTests(AppHostFixture fixture)
    {
        _client = fixture.ApiClient;
    }

    [Fact]
    public async Task GetTeams_Returns200()
    {
        var response = await _client.GetAsync("/api/teams");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostTeam_CreatesTeamAndReturns201()
    {
        var newTeam = new { Name = "Alpha Pack" };

        var response = await _client.PostAsJsonAsync("/api/teams", newTeam);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTeamById_ReturnsCreatedTeam()
    {
        var newTeam = new { Name = "Bravo Pack" };
        var createResponse = await _client.PostAsJsonAsync("/api/teams", newTeam);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!.ToString();
        var getResponse = await _client.GetAsync(location);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await getResponse.Content.ReadAsStringAsync();
        body.Should().Contain("Bravo Pack");
    }

    [Fact]
    public async Task GetTeamById_NonExistentId_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/teams/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
