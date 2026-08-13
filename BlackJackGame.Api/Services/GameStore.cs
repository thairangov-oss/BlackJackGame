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

        // Per-game sync objects to prevent concurrent mutations of the same game
        private readonly ConcurrentDictionary<Guid, object> _locks = new();

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

        // Executes the provided action while holding a per-game lock.
        // Returns the Game instance after the action (or throws KeyNotFoundException).
        public Game ExecuteWithLock(Guid id, Action<Game> action)
        {
            var sync = _locks.GetOrAdd(id, _ => new object());
            lock (sync)
            {
                if (!_games.TryGetValue(id, out var entry))
                    throw new KeyNotFoundException("Game not found");

                action(entry.game);

                // update last access time
                _games[id] = (entry.game, DateTime.UtcNow);

                return entry.game;
            }
        }

        // Removes old or completed games
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

            foreach (var id in toRemove) 
            {
                _games.TryRemove(id, out _);
                _locks.TryRemove(id, out _); // clean up lock objects
            }

            return toRemove.Count;
        }
    }
}
