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
    .WithHttpEndpoint(port: 5000, targetPort: 5001, name: "api-http");

// Add React/Vite frontend
builder.AddViteApp("web", "../DogTeams.Web/ClientApp", "start")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
