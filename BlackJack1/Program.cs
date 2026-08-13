using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BlackJack1
{
    public partial class Program
    {
        private static HttpClient _client = new HttpClient();

        public static async Task Main(string[] args)
        {
            // offline flag or env var enables mock mode
            bool offlineFlag = args.Any(a => a.Equals("--offline", StringComparison.OrdinalIgnoreCase));
            bool offlineEnv = Environment.GetEnvironmentVariable("BLACKJACK_OFFLINE") == "1";

            if (offlineFlag || offlineEnv)
            {
                Console.WriteLine("Starting in OFFLINE (mock) mode.");
                _client = new HttpClient(new MockHandler()) { BaseAddress = new Uri("http://offline/") };
            }
            else
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
                    selected = await PromptForApiUrlAsync(candidates);
                    if (selected == null)
                    {
                        Console.WriteLine("No API configured. Exiting.");
                        return;
                    }
                }

                _client = new HttpClient { BaseAddress = new Uri(selected) };
            }

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
                    // Offer to reconfigure URL if runtime networking fails
                    var newUrl = await PromptForApiUrlOnceAsync();
                    if (!string.IsNullOrWhiteSpace(newUrl))
                    {
                        _client = new HttpClient { BaseAddress = new Uri(newUrl) };
                        try
                        {
                            startResponse = await _client.PostAsJsonAsync("/api/game/start", bet);
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine($"Retry failed: {ex2.Message}");
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
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

        // --- Mock handler and minimal in-memory game logic used only for offline/demo mode ---
        private class MockHandler : HttpMessageHandler
        {
            private readonly ConcurrentDictionary<Guid, MockGame> _games = new();

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var path = request.RequestUri?.AbsolutePath ?? "/";
                if (request.Method == HttpMethod.Post && path.Equals("/api/game/start", StringComparison.OrdinalIgnoreCase))
                {
                    return HandleStartAsync(request);
                }

                if (request.Method == HttpMethod.Post && path.StartsWith("/api/game/") && path.EndsWith("/action"))
                {
                    return HandleActionAsync(request, path);
                }

                // Not found for other paths
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = JsonContent.Create(new { Message = "Not found (mock)" })
                });
            }

            private async Task<HttpResponseMessage> HandleStartAsync(HttpRequestMessage request)
            {
                int bet = 0;
                try
                {
                    var text = await request.Content.ReadAsStringAsync();
                    // client posts plain number or JSON number
                    if (int.TryParse(text, out var parsed)) bet = parsed;
                    else
                    {
                        try
                        {
                            var doc = JsonDocument.Parse(text);
                            if (doc.RootElement.ValueKind == JsonValueKind.Number && doc.RootElement.TryGetInt32(out var v))
                                bet = v;
                        }
                        catch { }
                    }
                }
                catch { }

                var game = MockGame.CreateInitial(bet);
                _games[game.Id] = game;

                var payload = new
                {
                    GameId = game.Id,
                    PlayerBalance = game.PlayerBalance,
                    PlayerHand = game.PlayerHand,
                    DealerHand = game.DealerHand,
                    DealerVisibleCard = game.DealerHand.Count > 0 ? game.DealerHand[0] : null,
                    PlayerScore = game.PlayerScore,
                    IsRoundComplete = game.IsComplete
                };

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(payload, options: new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })
                };
            }

            private async Task<HttpResponseMessage> HandleActionAsync(HttpRequestMessage request, string path)
            {
                var parts = path.Trim('/').Split('/');
                if (parts.Length >= 3 && Guid.TryParse(parts[2], out var gameId) && _games.TryGetValue(gameId, out var game))
                {
                    string action = "";
                    try
                    {
                        var txt = await request.Content.ReadAsStringAsync();
                        // Expecting JSON like {"Action":"Hit"}
                        var doc = JsonDocument.Parse(txt);
                        if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("Action", out var prop))
                            action = prop.GetString() ?? "";
                    }
                    catch { }

                    // Very minimal deterministic behavior
                    if (action.Equals("Hit", StringComparison.OrdinalIgnoreCase))
                    {
                        game.PlayerHand.Add("5 of Clubs");
                        game.PlayerScore += 5;
                        if (game.PlayerScore > 21)
                        {
                            game.IsComplete = true;
                            game.OutcomeMessage = "Player busts";
                        }
                    }
                    else if (action.Equals("Stand", StringComparison.OrdinalIgnoreCase))
                    {
                        // simple dealer logic: dealer hits to 17
                        while (game.DealerScore < 17)
                        {
                            game.DealerHand.Add("3 of Diamonds");
                            game.DealerScore += 3;
                        }
                        game.IsComplete = true;
                        if (game.PlayerScore > 21) game.OutcomeMessage = "Player busts";
                        else if (game.PlayerScore > game.DealerScore) game.OutcomeMessage = "Player wins";
                        else if (game.PlayerScore < game.DealerScore) game.OutcomeMessage = "Dealer wins";
                        else game.OutcomeMessage = "Push";
                    }
                    else if (action.Equals("Double", StringComparison.OrdinalIgnoreCase))
                    {
                        game.PlayerBet *= 2;
                        game.PlayerBalance -= game.PlayerBet / 2;
                        game.PlayerHand.Add("2 of Hearts");
                        game.PlayerScore += 2;
                        game.IsComplete = true;
                        if (game.PlayerScore > 21) game.OutcomeMessage = "Player busts";
                        else game.OutcomeMessage = "Player stands and wins (mock)";
                    }
                    else if (action.Equals("Insurance", StringComparison.OrdinalIgnoreCase))
                    {
                        game.InsuranceTaken = true;
                        game.PlayerBalance -= game.PlayerBet / 2;
                        game.OutcomeMessage = "Insurance taken (mock)";
                    }
                    else
                    {
                        // unknown action -> bad request
                        return new HttpResponseMessage(HttpStatusCode.BadRequest)
                        {
                            Content = JsonContent.Create(new { Message = "Unknown action (mock)" })
                        };
                    }

                    var payload = new
                    {
                        GameId = game.Id,
                        PlayerBalance = game.PlayerBalance,
                        PlayerHand = game.PlayerHand,
                        DealerHand = game.DealerHand,
                        PlayerScore = game.PlayerScore,
                        DealerScore = game.DealerScore,
                        OutcomeMessage = game.OutcomeMessage,
                        IsRoundComplete = game.IsComplete
                    };

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(payload)
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = JsonContent.Create(new { Message = "Game not found (mock)" })
                };
            }

            // Minimal mock game model
            private class MockGame
            {
                public Guid Id { get; set; }
                public int PlayerBalance { get; set; }
                public int PlayerBet { get; set; }
                public List<string> PlayerHand { get; set; } = new();
                public List<string> DealerHand { get; set; } = new();
                public int PlayerScore { get; set; }
                public int DealerScore { get; set; }
                public bool IsComplete { get; set; }
                public bool InsuranceTaken { get; set; }
                public string OutcomeMessage { get; set; } = "";

                public static MockGame CreateInitial(int bet)
                {
                    var g = new MockGame
                    {
                        Id = Guid.NewGuid(),
                        PlayerBet = bet,
                        PlayerBalance = 500 - Math.Max(0, bet)
                    };
                    g.PlayerHand.Add("10 of Hearts");
                    g.PlayerHand.Add("7 of Clubs");
                    g.PlayerScore = 17;
                    g.DealerHand.Add("King of Diamonds");
                    g.DealerHand.Add("6 of Clubs");
                    g.DealerScore = 16;
                    g.IsComplete = false;
                    return g;
                }
            }
        }

        // --- existing helpers remain unchanged below ---
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
                    Console.WriteLine($"Probing {c} ...");
                    using var handler = new HttpClientHandler();

                    // If you set BLACKJACK_IGNORE_SSL=1 in the environment, accept self-signed certs for localhost dev.
                    if (Environment.GetEnvironmentVariable("BLACKJACK_IGNORE_SSL") == "1")
                        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                    using var temp = new HttpClient(handler) { BaseAddress = new Uri(c) };

                    // Slightly longer timeout for local dev servers that may be slower to start
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

                    // Try a few common probe paths; any response (including 404) means the host is reachable.
                    var probePaths = new[] { "/", "/swagger/index.html", "/api/game/health" };
                    foreach (var p in probePaths)
                    {
                        try
                        {
                            var res = await temp.GetAsync(p, cts.Token);
                            Console.WriteLine($"  {c}{p} -> {(int)res.StatusCode} {res.ReasonPhrase}");
                            return c;
                        }
                        catch (OperationCanceledException)
                        {
                            Console.WriteLine($"  {c}{p} -> timed out");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  {c}{p} -> probe error: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Probe of {c} failed: {ex.Message}");
                }
            }
            return null;
        }

        private static async Task<string?> PromptForApiUrlAsync(string[] initialCandidates)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Unable to find a reachable Blackjack API. Choose an option:");
                Console.WriteLine("  (R)etry probing defaults");
                Console.WriteLine("  (U)se a custom URL (enter full base address)");
                Console.WriteLine("  (E)xit");
                Console.Write("Choice (R/U/E): ");
                var choice = (Console.ReadLine() ?? "").Trim().ToUpper();
                if (choice == "R")
                {
                    var sel = await FindWorkingBaseAddressAsync(initialCandidates);
                    if (sel != null) return sel;
                }
                else if (choice == "U")
                {
                    Console.Write("Enter API base URL (example: http://localhost:7178): ");
                    var input = (Console.ReadLine() ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(input)) continue;
                    // Quick probe of the provided URL
                    if (await ProbeUrlAsync(input))
                        return input;
                    Console.WriteLine("Provided URL did not respond to probes. Try again or start the API.");
                }
                else if (choice == "E")
                {
                    return null;
                }
            }
        }

        private static async Task<string?> PromptForApiUrlOnceAsync()
        {
            Console.Write("Enter an explicit API URL to retry or leave blank to cancel: ");
            var input = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(input)) return null;
            if (await ProbeUrlAsync(input)) return input;
            Console.WriteLine("Provided URL did not respond.");
            return null;
        }

        private static async Task<bool> ProbeUrlAsync(string url)
        {
            try
            {
                using var handler = new HttpClientHandler();
                if (Environment.GetEnvironmentVariable("BLACKJACK_IGNORE_SSL") == "1")
                    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                using var client = new HttpClient(handler) { BaseAddress = new Uri(url) };
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var probePaths = new[] { "/", "/swagger/index.html", "/api/game/health" };
                foreach (var p in probePaths)
                {
                    try
                    {
                        var res = await client.GetAsync(p, cts.Token);
                        Console.WriteLine($"  {url}{p} -> {(int)res.StatusCode} {res.ReasonPhrase}");
                        return true;
                    }
                    catch
                    {
                        // try next probe path
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Probe error: {ex.Message}");
            }
            return false;
        }
    }
}





