using Aspire.Hosting;
using Aspire.Hosting.Testing;

public class AppHostFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    public HttpClient ApiClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.DogTeams_AppHost>();

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        ApiClient = _app.CreateHttpClient("api");
    }

    public async Task DisposeAsync()
    {
        if (_app != null)
            await _app.DisposeAsync();
    }
}
