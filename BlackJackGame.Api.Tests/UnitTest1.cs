using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BlackJackGame.Api.Tests
{
    // Integration test for the Web API. Uses WebApplicationFactory<Program> which requires
    // the API project to expose the top-level Program class (default minimal API pattern).
    public class UnitTest1 : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public UnitTest1(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_Root_ReturnsSuccessOrNotFound()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/");

            // Accept 200 OK or 404 NotFound depending on whether root route exists.
            Assert.True(response.StatusCode == HttpStatusCode.OK
                        || response.StatusCode == HttpStatusCode.NotFound,
                        $"Unexpected status code: {response.StatusCode}");
        }
    }
}
