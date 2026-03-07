using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace DogTeams.Tests.Integration;

// Integration tests for /clubs endpoints.
// Skipped pending FB-1 ClubsController implementation.

public class ClubsIntegrationTests : IClassFixture<AppHostFixture>
{
    private readonly HttpClient _client;

    public ClubsIntegrationTests(AppHostFixture fixture)
    {
        _client = fixture.ApiClient;
    }

    [Fact(Skip = "Clubs controller not yet implemented (returns 501)")]
    public async Task GetAllClubs_ReturnsOk()
    {
        var response = await _client.GetAsync("/clubs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "Clubs controller not yet implemented (returns 501)")]
    public async Task CreateClub_ReturnsCreated()
    {
        var newClub = new
        {
            Name = "Cascadia Flyball Club",
            Region = "Northwest",
            HomeRegion = "PNW",
            NafaClubNumber = (string?)null
        };

        var response = await _client.PostAsJsonAsync("/clubs", newClub);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact(Skip = "Clubs controller not yet implemented (returns 501)")]
    public async Task GetClub_ById_ReturnsClub()
    {
        var newClub = new
        {
            Name = "River Valley Flyball",
            Region = "Southeast",
            HomeRegion = "SE"
        };

        var createResponse = await _client.PostAsJsonAsync("/clubs", newClub);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!.ToString();
        var getResponse = await _client.GetAsync(location);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await getResponse.Content.ReadAsStringAsync();
        body.Should().Contain("River Valley Flyball");
    }
}
