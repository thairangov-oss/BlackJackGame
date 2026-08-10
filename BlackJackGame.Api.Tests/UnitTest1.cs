using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace BlackJackGame.Api.Tests
{
    public class GameApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public GameApiTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _client = factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task StartGame_ReturnsInitialState()
        {
            var response = await _client.PostAsJsonAsync("/api/game/start", 25);

            var raw = await response.Content.ReadAsStringAsync();
            _output.WriteLine("StatusCode: " + (int)response.StatusCode + " " + response.StatusCode);
            _output.WriteLine("Response JSON: " + raw);

            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            bool hasGameId = TryGetPropertyIgnoreCase(root, "gameId", out var gameIdEl)
                             || TryGetPropertyIgnoreCase(root, "GameId", out gameIdEl)
                             || TryGetPropertyIgnoreCase(root, "id", out gameIdEl);

            Assert.True(hasGameId, $"Response JSON did not contain a game id. JSON: {raw}");

            // Optional: assert expected player balance (initial 500 - bet 25 => 475)
            if (TryGetPropertyIgnoreCase(root, "playerBalance", out var balEl) ||
                TryGetPropertyIgnoreCase(root, "PlayerBalance", out balEl))
            {
                if (balEl.ValueKind == JsonValueKind.Number && balEl.TryGetInt32(out var balance))
                {
                    Assert.Equal(475, balance);
                }
            }
        }

        private static bool TryGetPropertyIgnoreCase(JsonElement root, string name, out JsonElement value)
        {
            foreach (var prop in root.EnumerateObject())
            {
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = prop.Value;
                    return true;
                }
            }
            value = default;
            return false;
        }
    }
}
