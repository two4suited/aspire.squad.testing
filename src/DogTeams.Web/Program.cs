using System.IO;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.UseHttpsRedirection();

// Serve React SPA from ClientApp/dist
var clientAppPath = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "dist");

// Fallback to index.html for client-side routing
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/api") &&
        !context.Request.Path.Value?.Contains(".") == true)
    {
        context.Request.Path = "/index.html";
    }
    await next();
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(clientAppPath),
    RequestPath = ""
});

app.MapDefaultEndpoints();

// Catch-all: serve index.html for SPA routing
app.MapGet("/{*path}", async context =>
{
    var indexPath = Path.Combine(clientAppPath, "index.html");
    if (File.Exists(indexPath))
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(indexPath);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("SPA files not found. Run 'npm run build' in ClientApp directory.");
    }
});

app.Run();
