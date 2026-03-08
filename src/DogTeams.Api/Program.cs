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

// Only add Cosmos if connection is configured (makes it optional for standalone/test scenarios)
var cosmosConnectionString = builder.Configuration.GetConnectionString("cosmos");
var hasCosmosConfig = !string.IsNullOrEmpty(cosmosConnectionString) || 
                      builder.Configuration.GetSection("Aspire:Microsoft:Azure:Cosmos").Exists() ||
                      builder.Configuration.GetSection("Aspire:Microsoft:Azure:Cosmos:cosmos").Exists();

if (hasCosmosConfig)
{
    builder.AddAzureCosmosClient("cosmos");
    builder.Services.AddScoped<CosmosDbInitializer>();
}

builder.AddRedisClient("redis");

builder.Services.AddControllers();

builder.Services.Configure<CosmosDbOptions>(
    builder.Configuration.GetSection("CosmosDb"));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

// Register Cosmos DB context and repositories (only if Cosmos is configured)
if (hasCosmosConfig)
{
    builder.Services.AddScoped<CosmosDbContext>();
    builder.Services.AddScoped<ITeamRepository, TeamRepository>();
    builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();
    builder.Services.AddScoped<IDogRepository, DogRepository>();
    builder.Services.AddScoped<IUserService, CosmosUserService>();
}

builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

// Register auth services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

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

// Initialize Cosmos DB schema only if Cosmos is configured
if (hasCosmosConfig)
{
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
}
else
{
    app.Logger.LogWarning("Cosmos DB is not configured - running in standalone mode without data persistence");
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
