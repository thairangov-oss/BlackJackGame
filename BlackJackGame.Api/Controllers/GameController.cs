using BlackjackGame;
using BlackJackGame.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlackJackGame.Api.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameController : ControllerBase
    {
        [HttpPost("start")]
        public IActionResult Start([FromBody] int bet)
        {
            var game = new Game();
            game.Start(bet);
            var id = GameStore.AddGame(game);

            return Ok(new
            {
                GameId = id,
                PlayerBalance = game.Player.Balance,
                PlayerHand = game.Player.Hand.Cards.Select(c => c.ToString()),
                DealerHand = game.Dealer.Cards.Select(c => c.ToString()),
                IsRoundComplete = game.IsRoundComplete
            });
        }

        [HttpGet("{gameId}")]
        public IActionResult Get(Guid gameId)
        {
            var game = GameStore.GetGame(gameId);
            if (game == null) return NotFound();

            return Ok(new
            {
                PlayerBalance = game.Player.Balance,
                PlayerHand = game.Player.Hand.Cards.Select(c => c.ToString()),
                DealerHand = game.Dealer.Cards.Select(c => c.ToString()),
                IsRoundComplete = game.IsRoundComplete
            });
        }
    }
}

