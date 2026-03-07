using DogTeams.Api.Configuration;
using DogTeams.Api.Data;
using DogTeams.Api.Data.Repositories;
using DogTeams.Api.Auth;
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
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();
builder.Services.AddScoped<IDogRepository, DogRepository>();

// Register auth services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IUserService, InMemoryUserService>();

// Configure JWT authentication
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrEmpty(jwtOptions.SigningKey))
{
    // Generate a default key for development (32 bytes for HS256)
    jwtOptions.SigningKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
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
