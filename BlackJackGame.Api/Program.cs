using BlackJackGame.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Register GameStore as singleton
builder.Services.AddSingleton<GameStore>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Minimal health root so probing '/' returns 200 instead of 404
app.MapGet("/", () => Results.Ok(new { Status = "OK", Service = "BlackJack API" }));

app.Run();

// Add this so WebApplicationFactory<Program> can find the entry point in integration tests
public partial class Program { }
