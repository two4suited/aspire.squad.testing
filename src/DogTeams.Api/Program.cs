using DogTeams.Api.Configuration;
using DogTeams.Api.Data;
using DogTeams.Api.Data.Repositories;
using DogTeams.Api.Auth;
using DogTeams.Api.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddAzureCosmosClient("cosmos");
builder.AddRedisClient("redis");

builder.Services.AddControllers();

builder.Services.Configure<CosmosDbOptions>(
    builder.Configuration.GetSection("CosmosDb"));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

// Register Cosmos DB context and repositories
builder.Services.AddScoped<CosmosDbContext>();
builder.Services.AddScoped<CosmosDbInitializer>();
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();
builder.Services.AddScoped<IDogRepository, DogRepository>();

// Register auth services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IUserService, InMemoryUserService>();

// Configure JWT authentication
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

// In production, SigningKey must be configured
if (string.IsNullOrEmpty(jwtOptions.SigningKey))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("JWT SigningKey must be configured in production via appsettings.json or environment variables");
    
    // Generate a deterministic key for development (allows token persistence across restarts within the same session)
    jwtOptions.SigningKey = "ThisIsA32CharacterLongDevelopmentKeyForTesting!";
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

var app = builder.Build();

// Initialize Cosmos DB schema before the app processes any requests
try
{
    app.Logger.LogInformation("Starting Cosmos DB initialization...");
    using (var scope = app.Services.CreateScope())
    {
        var initializer = scope.ServiceProvider.GetRequiredService<CosmosDbInitializer>();
        app.Logger.LogInformation("Calling InitializeAsync()...");
        await initializer.InitializeAsync();
        app.Logger.LogInformation("Cosmos DB initialization completed successfully");
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Cosmos DB initialization failed - API startup aborted");
    throw;
}

// Seed test users for manual testing (development only)
if (app.Environment.IsDevelopment())
{
    app.Logger.LogInformation("Seeding test users for development...");
    InMemoryUserService.SeedTestUsers();
    app.Logger.LogInformation("Test users seeded successfully");
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();
