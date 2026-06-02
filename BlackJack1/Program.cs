//Review my project please

using System;
using System.Collections.Generic;

namespace BlackjackGame
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }
    public enum Rank
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack, Queen, King, Ace
    }

    public class Card
    {
        public Suit Suit { get; }
        public Rank Rank { get; }
        public Card(Suit suit, Rank rank) { Suit = suit; Rank = rank; }
        public override string ToString() => $"{Rank} of {Suit}";
    }

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

    public class Player
    {
        public Hand Hand { get; } = new();
        public int Balance { get; set; } = 500;
        public int Bet { get; set; }
        public bool InsuranceTaken { get; private set; }
        public void PlaceBet(int bet)
        {
            if (bet <= 0 || bet > Balance) throw new InvalidOperationException("Invalid bet.");
            Bet = bet; Balance -= Bet;
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

    public class Game
    {
        public Deck Deck { get; private set; }
        public Player Player { get; private set; }
        public Hand Dealer { get; private set; }
        public bool IsRoundComplete { get; set; }
        public Game() { Deck = new Deck(); Player = new Player(); Dealer = new Hand(); }

        public void ForceNewDeck(Deck deck) => Deck = deck;

        public void Start(int bet = 25)
        {
            IsRoundComplete = false;
            if (Deck == null || Deck.Cards.Count == 0) { Deck = new Deck(); Deck.Shuffle(); }
            Player.Hand.Cards.Clear(); Dealer = new Hand(); Player.ResetInsurance();
            Player.PlaceBet(bet);
            Player.Hand.AddCard(Deck.Deal()); Dealer.AddCard(Deck.Deal());
            Player.Hand.AddCard(Deck.Deal()); Dealer.AddCard(Deck.Deal());
            if (Player.Hand.CalculateScore() == 21)
            {
                if (Dealer.CalculateScore() == 21) Player.Balance += Player.Bet;
                else Player.Balance += (int)(Player.Bet * 2.5);
                IsRoundComplete = true;
            }
            else if (Dealer.CalculateScore() == 21)
            {
                if (Player.InsuranceTaken) Player.Balance += Player.Bet * 2;
                IsRoundComplete = true;
            }
        }
        public void DealerTurn() { while (Dealer.CalculateScore() < 17) Dealer.AddCard(Deck.Deal()); }
        public void CompareHands()
        {
            int playerValue = Player.Hand.CalculateScore();
            int dealerValue = Dealer.CalculateScore();
            if (playerValue > 21) return;
            if (dealerValue > 21 || playerValue > dealerValue) Player.Balance += Player.Bet * 2;
            else if (playerValue == dealerValue) Player.Balance += Player.Bet;
        }
    }

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
                    if (choice == "R") { game.Player.Balance = 500; Console.WriteLine("Balance reset to 500."); }
                    else return;
                }

                Console.WriteLine($"\nYour current balance: {game.Player.Balance}");
                Console.WriteLine("Choose your bet (5, 10, 25, 50, 100, 250): ");
                int bet;
                while (!int.TryParse(Console.ReadLine(), out bet) ||
                       (bet != 5 && bet != 10 && bet != 25 && bet != 50 && bet != 100 && bet != 250) ||
                       bet > game.Player.Balance)
                {
                    Console.WriteLine("Invalid bet. Try again.");
                }

                game.Start(bet);

                if (!game.IsRoundComplete)
                {
                    bool playerTurn = true;
                    while (playerTurn)
                    {
                        Console.WriteLine($"\nPlayer: {string.Join(", ", game.Player.Hand.Cards)} (Score: {game.Player.Hand.CalculateScore()})");
                        Console.WriteLine($"Dealer: {game.Dealer.Cards[0]} and [Hidden]");
                        Console.WriteLine("\nChoose action: (H)it, (S)tand, (D)ouble, (I)nsurance");
                        string choice = (Console.ReadLine() ?? "").ToUpper();

                        switch (choice)
                        {
                            case "H":
                                game.Player.Hit(game.Deck);
                                if (game.Player.Hand.CalculateScore() > 21)
                                {
                                    Console.WriteLine($"\nDealer: {string.Join(", ", game.Dealer.Cards)} (Score: {game.Dealer.CalculateScore()})");
                                    Console.WriteLine($"Player: {string.Join(", ", game.Player.Hand.Cards)} (Score: {game.Player.Hand.CalculateScore()})");
                                    Console.WriteLine($"Player busts with score {game.Player.Hand.CalculateScore()}!");
                                    Console.WriteLine($"Dealer score: {game.Dealer.CalculateScore()}");
                                    Console.WriteLine("Dealer wins!");
                                    game.IsRoundComplete = true; playerTurn = false;
                                }
                                break;
                            case "S": playerTurn = false; break;
                            case "D":
                                game.Player.Double(game.Deck);
                                if (game.Player.Hand.CalculateScore() > 21)
                                {
                                    Console.WriteLine($"\nDealer: {string.Join(", ", game.Dealer.Cards)} (Score: {game.Dealer.CalculateScore()})");
                                    Console.WriteLine($"Player: {string.Join(", ", game.Player.Hand.Cards)} (Score: {game.Player.Hand.CalculateScore()})");
                                    Console.WriteLine($"Player busts with score {game.Player.Hand.CalculateScore()}!");
                                    Console.WriteLine($"Dealer score: {game.Dealer.CalculateScore()}");
                                    Console.WriteLine("Dealer wins!");
                                    game.IsRoundComplete = true;
                                }
                                playerTurn = false; break;
                            case "I":
                                if (game.Dealer.Cards[0].Rank == Rank.Ace) game.Player.TakeInsurance();
                                else Console.WriteLine("Insurance only available if dealer shows Ace.");
                                break;
                            default: Console.WriteLine("Invalid choice."); break;
                        }
                    }
                    if (!game.IsRoundComplete) { game.DealerTurn(); game.CompareHands(); }
                }


                Console.WriteLine($"\nFinal Balance: {game.Player.Balance}");
                Console.WriteLine("\nDo you want to play again? (Y/N)");
                string replayChoice = (Console.ReadLine() ?? "").ToUpper();
                if (replayChoice != "Y") break;
            }
        }
    }
}
