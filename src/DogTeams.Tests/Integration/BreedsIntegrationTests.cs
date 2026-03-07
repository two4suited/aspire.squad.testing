using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

// Integration tests for /breeds endpoints.
// Skipped pending FB-1 BreedsController implementation.

public class BreedsIntegrationTests : IClassFixture<AppHostFixture>
{
    private readonly HttpClient _client;

    public BreedsIntegrationTests(AppHostFixture fixture)
    {
        _client = fixture.ApiClient;
    }

    [Fact(Skip = "Pending FB-1 controller implementation")]
    public async Task GetAllBreeds_ReturnsSeedData()
    {
        var response = await _client.GetAsync("/api/breeds");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        // Seed data includes Border Collie and Australian Shepherd at minimum
        body.Should().Contain("Border Collie");
        body.Should().Contain("Australian Shepherd");
    }

    [Fact(Skip = "Pending FB-1 controller implementation")]
    public async Task GetBreed_ById_ReturnsBreed()
    {
        // Fetch the breed list and retrieve the first breed by id
        var listResponse = await _client.GetAsync("/api/breeds");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var breeds = await listResponse.Content.ReadFromJsonAsync<List<BreedResponse>>();
        breeds.Should().NotBeNullOrEmpty();

        var firstId = breeds![0].Id;
        var getResponse = await _client.GetAsync($"/api/breeds/{firstId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await getResponse.Content.ReadAsStringAsync();
        body.Should().Contain(firstId);
    }

    private record BreedResponse(string Id, string Name);
}
