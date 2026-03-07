var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIRECOSMOSDB001
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator();
#pragma warning restore ASPIRECOSMOSDB001

var redis = builder.AddRedis("redis");

var api = builder.AddProject<Projects.DogTeams_Api>("api")
    .WithReference(cosmos)
    .WithReference(redis)
    .WaitFor(cosmos)
    .WaitFor(redis)
    .WithExternalHttpEndpoints();

// Add React/Vite frontend
builder.AddViteApp("web", "../DogTeams.Web/ClientApp", "start")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints()
    .WithEnvironment("VITE_API_URL", "http://localhost:5000/api");

builder.Build().Run();
