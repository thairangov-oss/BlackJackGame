using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BlackJackGame.Api.Tests
{
    public class GameApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public GameApiTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

        [Fact]
        public async Task StartGame_ReturnsInitialState()
        {
            var response = await _client.PostAsJsonAsync("/api/game/start", 25);
            response.EnsureSuccessStatusCode();

            var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(doc.TryGetProperty("gameId", out _));
            Assert.True(doc.TryGetProperty("playerBalance", out var balEl) && balEl.GetInt32() == 475);
        }
    }
}