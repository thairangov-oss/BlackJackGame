using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlackJack1
{
    public class Program
    {
        private static readonly HttpClient _client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7178") // Adjust to your API host/port
        };

        public static async Task Main()
        {
            Console.WriteLine("Welcome to Blackjack CLI (API-driven)");

            Guid gameId = Guid.Empty;

            while (true)
            {
                Console.Clear();

                // Prompt for bet
                Console.WriteLine("\nEnter your bet (5, 10, 25, 50, 100, 250): ");
                if (!int.TryParse(Console.ReadLine(), out int bet))
                {
                    Console.WriteLine("Invalid bet.");
                    continue;
                }

                // Start game via API
                var startResponse = await _client.PostAsJsonAsync("/api/game/start", bet);
                if (!startResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("Failed to start game.");
                    continue;
                }

                var startResult = await startResponse.Content.ReadFromJsonAsync<dynamic>();
                gameId = Guid.Parse((string)startResult.GameId);

                Console.WriteLine($"\nYour balance: {startResult.PlayerBalance}");
                Console.WriteLine($"Player Hand: {string.Join(", ", startResult.PlayerHand)} (Score: {startResult.PlayerScore})");
                Console.WriteLine($"Dealer Hand: {startResult.DealerVisibleCard} and [Hidden]");

                bool roundComplete = (bool)startResult.IsRoundComplete;

                // Player actions loop
                while (!roundComplete)
                {
                    Console.WriteLine("\nChoose action: (H)it, (S)tand, (D)ouble, (I)nsurance");
                    string choice = (Console.ReadLine() ?? "").ToUpper();

                    var actionResponse = await _client.PostAsJsonAsync($"/api/game/{gameId}/action", new { Action = choice });
                    if (!actionResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Action failed.");
                        break;
                    }

                    var actionResult = await actionResponse.Content.ReadFromJsonAsync<dynamic>();

                    // Narration from API response
                    Console.WriteLine($"\nPlayer Hand: {string.Join(", ", actionResult.PlayerHand)} (Score: {actionResult.PlayerScore})");
                    Console.WriteLine($"Dealer Hand: {string.Join(", ", actionResult.DealerHand)} (Score: {actionResult.DealerScore})");

                    Console.WriteLine($"Outcome: {actionResult.OutcomeMessage}");
                    Console.WriteLine($"Balance: {actionResult.PlayerBalance}");

                    roundComplete = (bool)actionResult.IsRoundComplete;
                }

                // Replay prompt
                Console.WriteLine("\nDo you want to play again? (Y/N)");
                string replayChoice = (Console.ReadLine() ?? "").ToUpper();
                if (replayChoice != "Y") break;
            }
        }
    }
}


