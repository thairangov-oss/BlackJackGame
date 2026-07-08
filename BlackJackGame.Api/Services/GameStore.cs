using BlackjackGame;

namespace BlackJackGame.Api.Services
{
    public static class GameStore
    {
        private static readonly Dictionary<Guid, Game> _games = new();

        public static Guid AddGame(Game game)
        {
            var id = Guid.NewGuid();
            _games[id] = game;
            return id;
        }

        public static Game? GetGame(Guid id) =>
            _games.TryGetValue(id, out var game) ? game : null;
    }

}
