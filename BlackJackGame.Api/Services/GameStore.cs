using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BlackJackCore.Core;

namespace BlackJackGame.Api.Services
{
    public sealed class GameStore
    {
        private readonly ConcurrentDictionary<Guid, (Game game, DateTime lastUpdated)> _games
            = new();

        public Guid Create(Game game)
        {
            var id = Guid.NewGuid();
            _games[id] = (game, DateTime.UtcNow);
            return id;
        }

        public bool TryGet(Guid id, out Game? game)
        {
            if (_games.TryGetValue(id, out var entry))
            {
                game = entry.game;
                // update last access time
                _games[id] = (entry.game, DateTime.UtcNow);
                return true;
            }

            game = null;
            return false;
        }

        public bool Remove(Guid id) => _games.TryRemove(id, out _);

        public int CleanupOldGames(TimeSpan maxAge)
        {
            var cutoff = DateTime.UtcNow - maxAge;
            var toRemove = new List<Guid>();

            foreach (var kvp in _games)
            {
                var id = kvp.Key;
                var (game, lastUpdated) = kvp.Value;
                if (lastUpdated < cutoff || (game != null && game.IsRoundComplete))
                {
                    toRemove.Add(id);
                }
            }

            foreach (var id in toRemove) _games.TryRemove(id, out _);
            return toRemove.Count;
        }
    }
}
