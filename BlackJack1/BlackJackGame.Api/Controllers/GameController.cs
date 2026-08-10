using System;
using System.Linq;
using System.Net;
using BlackJackCore.Core;
using BlackJackGame.Api.Model;
using BlackJackGame.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlackJackGame.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameStore _store;

        public GameController(GameStore store)
        {
            _store = store;
        }

        [HttpPost("start")]
        public IActionResult Start([FromBody] int bet)
        {
            if (bet <= 0) return BadRequest("Bet must be positive");

            var game = new Game();
            try
            {
                game.Start(bet);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            var id = _store.Create(game);
            return Ok(BuildResponse(id, game));
        }

        [HttpPost("{id:guid}/action")]
        public IActionResult Action(Guid id, [FromBody] ActionRequest req)
        {
            if (!_store.TryGet(id, out var game) || game == null) return NotFound();

            try
            {
                var action = (req?.Action ?? string.Empty).Trim().ToLowerInvariant();
                switch (action)
                {
                    case "hit": game.Hit(); break;
                    case "stand": game.Stand(); break;
                    case "double": game.Double(); break;
                    case "insurance": game.TakeInsurance(); break;
                    default: return BadRequest("Unknown action");
                }
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(ioe.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }

            return Ok(BuildResponse(id, game));
        }

        private GameResponse BuildResponse(Guid id, Game game)
        {
            string dealerVisible = game.Dealer.Hand.Cards.Count > 0 ? game.Dealer.Hand.Cards[0].ToString() : string.Empty;

            return new GameResponse
            {
                GameId = id,
                PlayerBalance = game.Player.Balance,
                PlayerHand = game.Player.Hand.Cards.Select(c => c.ToString()).ToArray(),
                DealerHand = game.Dealer.Hand.Cards.Select(c => c.ToString()).ToArray(),
                PlayerScore = game.Player.CalculateScore(),
                DealerScore = game.Dealer.CalculateScore(),
                DealerVisibleCard = dealerVisible,
                OutcomeMessage = game.OutcomeMessage,
                IsRoundComplete = game.IsRoundComplete
            };
        }
    }
}