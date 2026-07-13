using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlackJack1
{
    public class Program
    {
        private static HttpClient _client = new HttpClient();

        public static async Task Main(string[] args)
        {
            // Determine API URL (arg, env, or defaults)
            string argUrl = ParseArg(args, "--api-url");
            string envUrl = Environment.GetEnvironmentVariable("BLACKJACK_API_URL");
            var candidates = new[]
            {
                argUrl,
                envUrl,
                "http://localhost:5221",
                "http://localhost:7178",
                "https://localhost:7178"
            }.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToArray();

            var selected = await FindWorkingBaseAddressAsync(candidates);
            if (selected == null)
            {
                Console.WriteLine("Unable to find a reachable Blackjack API. Set BLACKJACK_API_URL or use --api-url.");
                return;
            }

            _client = new HttpClient { BaseAddress = new Uri(selected) };

            int currentBalance = 500;

            // Keep output contiguous (do not clear) so replay returns to the next line as requested
            Console.WriteLine("Welcome to Blackjack CLI (API-driven)");

            while (true)
            {
                // Print balance on its own line, bet prompt on the next line
                Console.WriteLine($"Your current balance: {currentBalance}");
                Console.Write("Choose your bet (5, 10, 25, 50, 100, 250): ");
                if (!int.TryParse(Console.ReadLine(), out int bet))
                {
                    Console.WriteLine("Invalid bet.");
                    continue;
                }

                // Start game
                HttpResponseMessage startResponse;
                try
                {
                    startResponse = await _client.PostAsJsonAsync("/api/game/start", bet);
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"\nUnable to connect to the Blackjack API: {ex.Message}");
                    continue;
                }

                if (!startResponse.IsSuccessStatusCode)
                {
                    var err = await startResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"\nStart failed: {(int)startResponse.StatusCode} {startResponse.ReasonPhrase}");
                    if (!string.IsNullOrWhiteSpace(err)) Console.WriteLine(err);
                    continue;
                }

                var startJson = await startResponse.Content.ReadAsStringAsync();
                using var startDoc = JsonDocument.Parse(startJson);
                var startRoot = startDoc.RootElement;

                if (!TryGetGuidIgnoreCase(startRoot, "GameId", out Guid gameId) || gameId == Guid.Empty)
                {
                    Console.WriteLine("Start response missing game id; aborting round.");
                    continue;
                }

                currentBalance = TryGetIntIgnoreCase(startRoot, "PlayerBalance", currentBalance);
                var playerHand = ReadStringArray(startRoot, "PlayerHand");
                var dealerHand = ReadStringArray(startRoot, "DealerHand");
                var playerScore = TryGetIntIgnoreCase(startRoot, "PlayerScore", -1);
                var isRoundComplete = TryGetBoolIgnoreCase(startRoot, "IsRoundComplete");

                // Initial display
                Console.WriteLine();
                Console.WriteLine($"Player: {string.Join(", ", playerHand)}{(playerScore >= 0 ? $" (Score: {playerScore})" : "")}");
                string visible = TryGetStringIgnoreCase(startRoot, "DealerVisibleCard") ?? (dealerHand.Count > 0 ? dealerHand[0] : "[Hidden]");
                Console.WriteLine($"Dealer: {visible} and [Hidden]");

                // Player action loop
                while (!isRoundComplete)
                {
                    Console.Write("Choose action: (H)it, (S)tand, (D)ouble, (I)nsurance ");
                    string raw = (Console.ReadLine() ?? "").Trim().ToUpper();
                    string action = raw switch
                    {
                        "H" => "Hit",
                        "S" => "Stand",
                        "D" => "Double",
                        "I" => "Insurance",
                        "HIT" => "Hit",
                        "STAND" => "Stand",
                        "DOUBLE" => "Double",
                        "INSURANCE" => "Insurance",
                        _ => null
                    };
                    if (action == null)
                    {
                        Console.WriteLine("Invalid action.");
                        continue;
                    }

                    HttpResponseMessage actionResponse;
                    try
                    {
                        actionResponse = await _client.PostAsJsonAsync($"/api/game/{gameId}/action", new { Action = action });
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"\nNetwork error: {ex.Message}");
                        break;
                    }

                    if (!actionResponse.IsSuccessStatusCode)
                    {
                        var err = await actionResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"\nAction failed: {(int)actionResponse.StatusCode} {actionResponse.ReasonPhrase}");
                        if (!string.IsNullOrWhiteSpace(err)) Console.WriteLine(err);
                        break;
                    }

                    var actionJson = await actionResponse.Content.ReadAsStringAsync();
                    using var actionDoc = JsonDocument.Parse(actionJson);
                    var actionRoot = actionDoc.RootElement;

                    currentBalance = TryGetIntIgnoreCase(actionRoot, "PlayerBalance", currentBalance);
                    var pHand = ReadStringArray(actionRoot, "PlayerHand");
                    var dHand = ReadStringArray(actionRoot, "DealerHand");
                    var pScore = TryGetIntIgnoreCase(actionRoot, "PlayerScore", -1);
                    var dScore = TryGetIntIgnoreCase(actionRoot, "DealerScore", -1);
                    var outcome = TryGetStringIgnoreCase(actionRoot, "OutcomeMessage") ?? string.Empty;
                    isRoundComplete = TryGetBoolIgnoreCase(actionRoot, "IsRoundComplete");

                    // In-round display or final display
                    Console.WriteLine();
                    Console.WriteLine($"Player: {string.Join(", ", pHand)}{(pScore >= 0 ? $" (Score: {pScore})" : "")}");
                    if (!isRoundComplete)
                    {
                        string vis = dHand.Count > 0 ? dHand[0] : "[Hidden]";
                        Console.WriteLine($"Dealer: {vis} and [Hidden]");
                    }
                    else
                    {
                        Console.WriteLine($"Dealer: {string.Join(", ", dHand)} (Score: {(dScore >= 0 ? dScore.ToString() : "N/A")})");
                        var finalMsg = NormalizeOutcome(outcome, pScore, dScore);
                        Console.WriteLine(finalMsg);
                        Console.WriteLine($"Final Balance: {currentBalance}");
                    }
                }

                Console.Write("Do you want to play again? (Y/N) ");
                var replay = (Console.ReadLine() ?? "").Trim().ToUpper();
                if (replay != "Y") break;
                // continue loop — since we do not clear, next iteration prints balance on the next line as requested
            }
        }

        // Helpers

        private static string NormalizeOutcome(string outcome, int pScore, int dScore)
        {
            if (string.IsNullOrWhiteSpace(outcome))
            {
                if (pScore > 21) outcome = "Player busts";
                else if (pScore > dScore) outcome = "Player wins";
                else if (pScore < dScore) outcome = "Dealer wins";
                else outcome = "Push";
            }
            // Convert trailing punctuation rules: replace '.' with '!' and ensure ends with '!'
            outcome = outcome.Trim();
            outcome = outcome.Replace(".", "!");
            if (!outcome.EndsWith("!")) outcome += "!";
            // If player busted and outcome does not mention dealer, append Dealer wins!
            if (pScore > 21 && !outcome.Contains("Dealer", StringComparison.OrdinalIgnoreCase))
            {
                outcome += " Dealer wins!";
            }
            return outcome;
        }

        private static bool TryGetPropertyIgnoreCase(JsonElement root, string name, out JsonElement value)
        {
            foreach (var prop in root.EnumerateObject())
            {
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = prop.Value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        private static bool TryGetGuidIgnoreCase(JsonElement root, string name, out Guid result)
        {
            result = Guid.Empty;
            if (TryGetPropertyIgnoreCase(root, name, out var el) && el.ValueKind == JsonValueKind.String)
            {
                Guid.TryParse(el.GetString(), out result);
                return result != Guid.Empty;
            }
            return false;
        }

        private static int TryGetIntIgnoreCase(JsonElement root, string name, int fallback)
        {
            if (TryGetPropertyIgnoreCase(root, name, out var el) && el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var v))
                return v;
            return fallback;
        }

        private static bool TryGetBoolIgnoreCase(JsonElement root, string name)
        {
            if (TryGetPropertyIgnoreCase(root, name, out var el))
            {
                return el.ValueKind == JsonValueKind.True;
            }
            return false;
        }

        private static string? TryGetStringIgnoreCase(JsonElement root, string name)
        {
            if (TryGetPropertyIgnoreCase(root, name, out var el) && el.ValueKind == JsonValueKind.String)
                return el.GetString();
            return null;
        }

        private static List<string> ReadStringArray(JsonElement root, string propertyName)
        {
            var list = new List<string>();
            if (TryGetPropertyIgnoreCase(root, propertyName, out var prop) && prop.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in prop.EnumerateArray())
                {
                    if (el.ValueKind == JsonValueKind.String)
                        list.Add(el.GetString()!);
                    else
                        list.Add(el.ToString());
                }
            }
            return list;
        }

        private static string ParseArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    return args[i + 1];
                if (args[i].StartsWith(name + "=", StringComparison.OrdinalIgnoreCase))
                    return args[i].Substring(name.Length + 1);
            }
            return null;
        }

        private static async Task<string?> FindWorkingBaseAddressAsync(string[] candidates)
        {
            foreach (var c in candidates)
            {
                try
                {
                    using var temp = new HttpClient { BaseAddress = new Uri(c) };
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                    await temp.GetAsync("/", cts.Token);
                    return c;
                }
                catch
                {
                    // try next
                }
            }
            return null;
        }
    }
}


