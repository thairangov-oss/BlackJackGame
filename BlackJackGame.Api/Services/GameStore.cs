using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BlackJackCore.Core;

namespace BlackJackGame.Api.Services
{
    public static class GameStore
    {
        private static readonly ConcurrentDictionary<Guid, Game> _games = new();

        public static Guid AddGame(Game game)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));
            var id = Guid.NewGuid();
            _games.TryAdd(id, game);
            return id;
        }

        public static Game? GetGame(Guid id) =>
            _games.TryGetValue(id, out var game) ? game : null;

        public static bool TryRemove(Guid id, out Game? removed) =>
            _games.TryRemove(id, out removed);

        public static IEnumerable<KeyValuePair<Guid, Game>> GetAllGamesSnapshot() => _games.ToArray();
    }
}
