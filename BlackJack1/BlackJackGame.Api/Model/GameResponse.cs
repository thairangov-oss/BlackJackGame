using System;

namespace BlackJackGame.Api.Model
{
    public class GameResponse
    {
        public Guid GameId { get; set; }
        public int PlayerBalance { get; set; }
        public string[] PlayerHand { get; set; } = Array.Empty<string>();
        public string[] DealerHand { get; set; } = Array.Empty<string>();
        public int PlayerScore { get; set; }
        public int DealerScore { get; set; }
        public string DealerVisibleCard { get; set; } = string.Empty;
        public string OutcomeMessage { get; set; } = string.Empty;
        public bool IsRoundComplete { get; set; }
    }
}