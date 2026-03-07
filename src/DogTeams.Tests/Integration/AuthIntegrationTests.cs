using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

// NOTE: These tests require the full Aspire host (DogTeams.AppHost + DogTeams.Api).
// They are skipped until those projects are available and the AppHostFixture is wired up.

public class AuthIntegrationTests : IClassFixture<AppHostFixture>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(AppHostFixture fixture)
    {
        _client = fixture.ApiClient;
    }

    [Fact(Skip = "Requires DogTeams.AppHost and DogTeams.Api — remove Skip when projects are available")]
    public async Task PostAuthRegister_CreatesUser()
    {
        var registration = new
        {
            Email = $"owner-{Guid.NewGuid()}@example.com",
            Password = "P@ssw0rd!",
            DisplayName = "Test Owner"
        };

        var response = await _client.PostAsJsonAsync("/auth/register", registration);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    [Fact(Skip = "Requires DogTeams.AppHost and DogTeams.Api — remove Skip when projects are available")]
    public async Task PostAuthLogin_ReturnsJwtToken()
    {
        var email = $"owner-{Guid.NewGuid()}@example.com";
        var password = "P@ssw0rd!";

        await _client.PostAsJsonAsync("/auth/register", new
        {
            Email = email,
            Password = password,
            DisplayName = "Login Test Owner"
        });

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new
        {
            Email = email,
            Password = password
        });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await loginResponse.Content.ReadAsStringAsync();
        body.Should().Contain("token");
    }

    [Fact(Skip = "Requires DogTeams.AppHost and DogTeams.Api — remove Skip when projects are available")]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        // /teams is a protected endpoint — accessing without auth should be unauthorized
        var client = new HttpClient { BaseAddress = _client.BaseAddress };

        var response = await client.GetAsync("/teams");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "Requires DogTeams.AppHost and DogTeams.Api — remove Skip when projects are available")]
    public async Task ProtectedEndpoint_WithValidToken_Returns200()
    {
        var email = $"owner-{Guid.NewGuid()}@example.com";
        var password = "P@ssw0rd!";

        await _client.PostAsJsonAsync("/auth/register", new
        {
            Email = email,
            Password = password,
            DisplayName = "Auth Test Owner"
        });

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new
        {
            Email = email,
            Password = password
        });

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginBody.Should().NotBeNull();
        loginBody!.Token.Should().NotBeNullOrEmpty();

        var authedClient = new HttpClient { BaseAddress = _client.BaseAddress };
        authedClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.Token);

        var response = await authedClient.GetAsync("/teams");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private record LoginResponse(string Token);
}
