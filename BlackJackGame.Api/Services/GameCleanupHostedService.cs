using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlackJackGame.Api.Services
{
    public sealed class GameCleanupHostedService : BackgroundService
    {
        private readonly GameStore _gameStore;
        private readonly ILogger<GameCleanupHostedService> _logger;
        private readonly TimeSpan _interval;
        private readonly TimeSpan _maxAge;

        public GameCleanupHostedService(GameStore gameStore, ILogger<GameCleanupHostedService> logger)
        {
            _gameStore = gameStore ?? throw new ArgumentNullException(nameof(gameStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _interval = TimeSpan.FromMinutes(5);   // run cleanup every 5 minutes (adjust as needed)
            _maxAge = TimeSpan.FromMinutes(30);    // remove games older than 30 minutes (adjust as needed)
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(_interval);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    try
                    {
                        var removed = _gameStore.CleanupOldGames(_maxAge);
                        if (removed > 0)
                        {
                            _logger.LogInformation("GameCleanupHostedService removed {Count} old/complete game(s).", removed);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while executing game cleanup.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // shutdown requested
            }
        }
    }
}
