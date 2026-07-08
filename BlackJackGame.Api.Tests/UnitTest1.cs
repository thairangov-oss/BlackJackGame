using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace BlackJackGame.Api.Tests
{
    [TestClass]
    public class UnitTest1
    {
        public class GameApiTests : IClassFixture<WebApplicationFactory<Program>>
        {
            private readonly HttpClient _client;

            public GameApiTests(WebApplicationFactory<Program> factory)
            {
                _client = factory.CreateClient();
            }

            [Fact]
            public async Task StartGame_ReturnsInitialState()
            {
                var response = await _client.PostAsJsonAsync("/api/game/start", 25);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                Assert.NotNull(result.GameId);
                Assert.Equal(475, (int)result.PlayerBalance);
            }
        }

    }
}
