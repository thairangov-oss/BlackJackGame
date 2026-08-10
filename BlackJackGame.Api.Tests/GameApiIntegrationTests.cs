using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BlackJackGame.Api.Tests
{
    public class GameApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public GameApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task StartEndpoint_Returns_OK_WithGameId()
        {
            var client = _factory.CreateClient();

            // If Start expects an int in body, use the raw int. Adjust if Start signature changes.
            var response = await client.PostAsJsonAsync("/api/game/start", 25);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            Assert.True(json.TryGetProperty("GameId", out var idProp));
            Assert.True(Guid.TryParse(idProp.GetString(), out _));
        }
    }
}
