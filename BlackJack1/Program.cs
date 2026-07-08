using System;
using System.Collections.Generic;
using System.Linq;

namespace BlackjackGame
{
    /// <summary>
    /// Represents the four suits in a standard deck of cards.
    /// </summary>
    public enum Suit { Hearts, Diamonds, Clubs, Spades }

    /// <summary>
    /// Represents the ranks of cards in a standard deck.
    /// </summary>
    public enum Rank
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack, Queen, King, Ace
    }

    /// <summary>
    /// Represents a playing card with a suit and rank.
    /// </summary>
    public class Card
    {
        public Suit Suit { get; }
        public Rank Rank { get; }
        public Card(Suit suit, Rank rank) { Suit = suit; Rank = rank; }
        public override string ToString() => $"{Rank} of {Suit}";
    }

    /// <summary>
    /// Provides helper methods for determining card values.
    /// </summary>
    public static class CardValueHelper
    {
        public static int GetCardValue(Rank rank) =>
            rank switch
            {
                Rank.Jack or Rank.Queen or Rank.King => 10,
                Rank.Ace => 11,
                _ => (int)rank
            };
    }

    /// <summary>
    /// Contains constants used throughout the Blackjack game.
    /// </summary>
    public static class GameConstants
    {
        public const int DefaultBalance = 500;
        public const int DefaultBet = 25;
        public static readonly int[] AllowedBets = { 5, 10, 25, 50, 100, 250 };
        public const double BlackjackPayoutMultiplier = 2.5;
    }

    /// <summary>
    /// Represents a deck of playing cards.
    /// </summary>
    public class Deck
    {
        public List<Card> Cards { get; private set; }
        private static readonly Random rng = Random.Shared;
        public Deck()
        {
            Cards = new List<Card>();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    Cards.Add(new Card(suit, rank));
        }
        public void Shuffle()
        {
            for (int i = Cards.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (Cards[i], Cards[j]) = (Cards[j], Cards[i]);
            }
        }
        public Card Deal()
        {
            Card card = Cards[0];
            Cards.RemoveAt(0);
            return card;
        }
    }

    /// <summary>
    /// Represents a hand of cards held by a player or dealer.
    /// </summary>
    public class Hand
    {
        public List<Card> Cards { get; } = new();
        public void AddCard(Card card) => Cards.Add(card);
        public int CalculateScore()
        {
            int total = 0, aceCount = 0;
            foreach (var card in Cards)
            {
                int value = CardValueHelper.GetCardValue(card.Rank);
                total += value;
                if (card.Rank == Rank.Ace) aceCount++;
            }
            while (total > 21 && aceCount > 0) { total -= 10; aceCount--; }
            return total;
        }
    }

    /// <summary>
    /// Encapsulates the result of a bet attempt.
    /// </summary>
    public class BetResult
    {
        public bool Success { get; }
        public string ErrorMessage { get; }
        public BetResult(bool success, string errorMessage = "")
        {
            Success = success;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// Represents a player in the Blackjack game.
    /// </summary>
    public class Player
    {
        public Hand Hand { get; } = new();
        public int Balance { get; set; } = GameConstants.DefaultBalance;
        public int Bet { get; set; }
        public bool InsuranceTaken { get; private set; }

        public BetResult PlaceBet(int bet)
        {
            if (bet <= 0 || bet > Balance)
                return new BetResult(false, "Invalid bet amount.");
            Bet = bet;
            Balance -= Bet;
            return new BetResult(true);
        }

        public void Hit(Deck deck) => Hand.AddCard(deck.Deal());
        public void Double(Deck deck)
        {
            if (Hand.Cards.Count == 2 && Balance >= Bet)
            {
                Balance -= Bet; Bet *= 2; Hit(deck);
            }
        }
        public void TakeInsurance()
        {
            if (!InsuranceTaken && Balance >= Bet / 2)
            {
                Balance -= Bet / 2; InsuranceTaken = true;
            }
        }
        public void ResetInsurance() => InsuranceTaken = false;
    }

    /// <summary>
    /// Represents the overall Blackjack game engine.
    /// </summary>
    public class Game
    {
        public Deck Deck { get; private set; }
        public Player Player { get; private set; }
        public Hand Dealer { get; private set; }
        public bool IsRoundComplete { get; set; }
        public Game() { Deck = new Deck(); Player = new Player(); Dealer = new Hand(); }

        public void ForceNewDeck(Deck deck) => Deck = deck;

        public void Start(int bet = GameConstants.DefaultBet)
        {
            IsRoundComplete = false;
            if (Deck == null || Deck.Cards.Count == 0) { Deck = new Deck(); Deck.Shuffle(); }
            Player.Hand.Cards.Clear(); Dealer = new Hand(); Player.ResetInsurance();

            var betResult = Player.PlaceBet(bet);
            if (!betResult.Success)
            {
                Console.WriteLine(betResult.ErrorMessage);
                IsRoundComplete = true;
                return;
            }

            Player.Hand.AddCard(Deck.Deal()); Dealer.AddCard(Deck.Deal());
            Player.Hand.AddCard(Deck.Deal()); Dealer.AddCard(Deck.Deal());

            if (Player.Hand.CalculateScore() == 21)
            {
                if (Dealer.CalculateScore() == 21) Player.Balance += Player.Bet;
                else Player.Balance += (int)(Player.Bet * GameConstants.BlackjackPayoutMultiplier);
                IsRoundComplete = true;
            }
            else if (Dealer.CalculateScore() == 21)
            {
                if (Player.InsuranceTaken)
                {
                    Player.Balance += Player.Bet; // insurance pays 2:1 on half bet
                }
                IsRoundComplete = true;
            }
        }

        private bool DealerShouldHit(Hand dealer)
        {
            int score = dealer.CalculateScore();
            bool hasSoftAce = dealer.Cards.Any(c => c.Rank == Rank.Ace) &&
                              dealer.Cards.Sum(c => CardValueHelper.GetCardValue(c.Rank)) == 17;
            return score < 17 || (score == 17 && hasSoftAce);
        }

        public void DealerTurn() { while (DealerShouldHit(Dealer)) Dealer.AddCard(Deck.Deal()); }

        public void CompareHands()
        {
            int playerValue = Player.Hand.CalculateScore();
            int dealerValue = Dealer.CalculateScore();
            if (playerValue > 21) return;
            if (dealerValue > 21 || playerValue > dealerValue) Player.Balance += Player.Bet * 2;
            else if (playerValue == dealerValue) Player.Balance += Player.Bet;
        }
    }

    /// <summary>
    /// Provides console-based UI helper methods.
    /// </summary>
    public static class ConsoleUI
    {
        public static void ShowHand(string label, Hand hand)
        {
            Console.WriteLine($"{label}: {string.Join(", ", hand.Cards)} (Score: {hand.CalculateScore()})");
        }

        public static int PromptBet(Player player)
        {
            Console.WriteLine("Choose your bet (5, 10, 25, 50, 100, 250): ");
            int bet;
            while (!int.TryParse(Console.ReadLine(), out bet) ||
                   !GameConstants.AllowedBets.Contains(bet) ||
                   bet > player.Balance)
            {
                Console.WriteLine("Invalid bet. Try again.");
            }
            return bet;
        }
    }

    /// <summary>
    /// Entry point for the Blackjack console application.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Game game = new Game();
            while (true)
            {
                Console.Clear();
                if (game.Player.Balance <= 0)
                {
                    Console.WriteLine("\nYou are bust! Retry (R) or press any key to close.");
                    string choice = (Console.ReadLine() ?? "").ToUpper();
                    if (choice == "R") { game.Player.Balance = GameConstants.DefaultBalance; Console.WriteLine("Balance reset."); }
                    else return;
                }

                Console.WriteLine($"\nYour current balance: {game.Player.Balance}");
                int bet = ConsoleUI.PromptBet(game.Player);
                game.Start(bet);

                if (!game.IsRoundComplete)
                {
                    bool playerTurn = true;
                    while (playerTurn)
                    {
                        ConsoleUI.ShowHand("Player", game.Player.Hand);
                        Console.WriteLine($"Dealer: {game.Dealer.Cards[0]} and [Hidden]");
                        Console.WriteLine("\nChoose action: (H)it, (S)tand, (D)ouble, (I)nsurance");
                        string choice = (Console.ReadLine() ?? "").ToUpper();

                        switch (choice)
                        {
                            case "H":
                                game.Player.Hit(game.Deck);
                                if (game.Player.Hand.CalculateScore() > 21)
                                {
                                    ConsoleUI.ShowHand("Player", game.Player.Hand);
                                    ConsoleUI.ShowHand("Dealer", game.Dealer);
                                    // 🔄 Restored narration for bust outcome
                                    Console.WriteLine($"Player busts with {game.Player.Hand.CalculateScore()}. Dealer wins!");
                                    game.IsRoundComplete = true;
                                    playerTurn = false;
                                }
                                break;
                            case "S":
                                playerTurn = false;
                                break;
                            case "D":
                                game.Player.Double(game.Deck);
                                if (game.Player.Hand.CalculateScore() > 21)
                                {
                                    ConsoleUI.ShowHand("Player", game.Player.Hand);
                                    ConsoleUI.ShowHand("Dealer", game.Dealer);
                                    // 🔄 Restored narration for bust outcome
                                    Console.WriteLine($"Player busts with {game.Player.Hand.CalculateScore()}. Dealer wins!");
                                    game.IsRoundComplete = true;
                                }
                                playerTurn = false;
                                break;
                            case "I":
                                if (game.Dealer.Cards[0].Rank == Rank.Ace)
                                    game.Player.TakeInsurance();
                                else
                                    Console.WriteLine("Insurance only available if dealer shows Ace.");
                                break;
                            default:
                                Console.WriteLine("Invalid choice.");
                                break;
                        }
                    }

                    if (!game.IsRoundComplete)
                    {
                        game.DealerTurn();
                        game.CompareHands();

                        // 🔄 Restored final narration block
                        ConsoleUI.ShowHand("Player", game.Player.Hand);
                        ConsoleUI.ShowHand("Dealer", game.Dealer);

                        int playerScore = game.Player.Hand.CalculateScore();
                        int dealerScore = game.Dealer.CalculateScore();

                        if (playerScore > 21)
                        {
                            Console.WriteLine($"Player busts with {playerScore}. Dealer wins!");
                        }
                        else if (dealerScore > 21)
                        {
                            Console.WriteLine($"Dealer busts with {dealerScore}. Player wins!");
                        }
                        else if (playerScore > dealerScore)
                        {
                            Console.WriteLine($"Player wins {playerScore} vs {dealerScore}!");
                        }
                        else if (playerScore < dealerScore)
                        {
                            Console.WriteLine($"Dealer wins {dealerScore} vs {playerScore}!");
                        }
                        else
                        {
                            Console.WriteLine($"Push: both scored {playerScore}.");
                        }
                    }
                }

                Console.WriteLine($"\nFinal Balance: {game.Player.Balance}");
                Console.WriteLine("\nDo you want to play again? (Y/N)");
                string replayChoice = (Console.ReadLine() ?? "").ToUpper();
                if (replayChoice != "Y") break;
            }
        }
    }
}

