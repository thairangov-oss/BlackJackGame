using System;
using System.Linq;
using BlackJackCore.Core;
using BlackJackGame.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlackJackGame.Api.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameController : ControllerBase
    {
        // -------------------
        // Phase 1 Endpoints
        // -------------------

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
                DealerHand = game.Dealer.Hand.Cards.Select(c => c.ToString()),
                // Expose visible dealer card separately for CLI
                DealerVisibleCard = game.Dealer.Hand.Cards.FirstOrDefault()?.ToString(),
                PlayerScore = game.Player.CalculateScore(),
                IsRoundComplete = game.IsRoundComplete,
                OutcomeMessage = game.OutcomeMessage
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
                DealerHand = game.Dealer.Hand.Cards.Select(c => c.ToString()),
                IsRoundComplete = game.IsRoundComplete
            });
        }

        // -------------------
        // Phase 2 Endpoints
        // -------------------

        [HttpPost("{gameId}/action")]
        public IActionResult PerformAction(Guid gameId, [FromBody] ActionRequest request)
        {
            var game = GameStore.GetGame(gameId);
            if (game == null) return NotFound();

            try
            {
                switch (request.Action.ToLower())
                {
                    case "hit":
                        game.Hit();
                        break;
                    case "stand":
                        game.Stand();
                        break;
                    case "double":
                        game.Double();
                        break;
                    case "insurance":
                        game.TakeInsurance();
                        break;
                    default:
                        return BadRequest(new { message = "Invalid action" });
                }

                return Ok(new
                {
                    PlayerBalance = game.Player.Balance,
                    PlayerHand = game.Player.Hand.Cards.Select(c => c.ToString()),
                    DealerHand = game.Dealer.Hand.Cards.Select(c => c.ToString()),
                    IsRoundComplete = game.IsRoundComplete,
                    OutcomeMessage = game.OutcomeMessage,
                    PlayerScore = game.Player.CalculateScore(),
                    DealerScore = game.Dealer.CalculateScore()
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // DTO for Phase 2
    public class ActionRequest
    {
        public string Action { get; set; }
    }
}


