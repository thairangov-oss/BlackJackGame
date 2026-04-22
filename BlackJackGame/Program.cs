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

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public override string ToString() => $"{Rank} of {Suit}";
    }

    public static class CardValueHelper
    {
        public static int GetCardValue(Rank rank)
        {
            switch (rank)
            {
                case Rank.Jack:
                case Rank.Queen:
                case Rank.King:
                    return 10;
                case Rank.Ace:
                    return 11;
                default:
                    return (int)rank; // explicit enum values
            }
        }
    }

    public class Deck
    {
        public List<Card> Cards { get; private set; }
        private static readonly Random rng = Random.Shared;

        public Deck()
        {
            Cards = new List<Card>();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    Cards.Add(new Card(suit, rank));
                }
            }
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
        public List<Card> Cards { get; } = new List<Card>();

        public void AddCard(Card card) => Cards.Add(card);

        public int CalculateScore()
        {
            int total = 0;
            int aceCount = 0;

            foreach (var card in Cards)
            {
                int value = CardValueHelper.GetCardValue(card.Rank);
                total += value;
                if (card.Rank == Rank.Ace) aceCount++;
            }

            while (total > 21 && aceCount > 0)
            {
                total -= 10;
                aceCount--;
            }

            return total;
        }
    }

    public class Player
    {
        public Hand Hand { get; } = new Hand();
        public int Balance { get; set; } = 500;
        public int Bet { get; set; }
        public bool InsuranceTaken { get; private set; }

        public void PlaceBet()
        {
            Console.WriteLine($"\nYour current balance: {Balance}");
            Console.WriteLine("Choose your bet (5, 10, 25, 50, 100, 250): ");
            int bet;
            while (!int.TryParse(Console.ReadLine(), out bet) ||
                   (bet != 5 && bet != 10 && bet != 25 && bet != 50 && bet != 100 && bet != 250) ||
                   bet > Balance)
            {
                Console.WriteLine("Invalid bet. Try again.");
            }
            Bet = bet;
            Balance -= Bet;
            Console.WriteLine($"Bet placed: {Bet}. Remaining balance: {Balance}");
        }

        public void Hit(Deck deck) => Hand.AddCard(deck.Deal());

        public void Double(Deck deck)
        {
            if (Hand.Cards.Count == 2 && Balance >= Bet)
            {
                Balance -= Bet;
                Bet *= 2;
                Hit(deck);
            }
            else Console.WriteLine("Double down only allowed on initial two cards.");
        }

        public void Insurance()
        {
            if (!InsuranceTaken && Balance >= Bet / 2)
            {
                Balance -= Bet / 2;
                InsuranceTaken = true;
                Console.WriteLine("Insurance taken.");
            }
            else Console.WriteLine("Insurance not available.");
        }

        public void ResetInsurance() => InsuranceTaken = false;
    }

    public class Game
    {
        public Deck Deck { get; private set; }
        public Player Player { get; set; }
        public Hand Dealer { get; private set; }
        public bool IsRoundComplete { get; set; }

        public Game()
        {
            Deck = new Deck();
            Player = new Player();
            Dealer = new Hand();
        }

        public void Start()
        {
            IsRoundComplete = false;
            Deck = new Deck();
            Deck.Shuffle();

            Player.Hand.Cards.Clear();
            Dealer = new Hand();
            Player.ResetInsurance();

            Player.PlaceBet();

            Player.Hand.AddCard(Deck.Deal());
            Dealer.AddCard(Deck.Deal());
            Player.Hand.AddCard(Deck.Deal());
            Dealer.AddCard(Deck.Deal());

            Console.WriteLine($"Player: {string.Join(", ", Player.Hand.Cards)} (Score: {Player.Hand.CalculateScore()})");
            Console.WriteLine($"Dealer: {Dealer.Cards[0]} and [Hidden]");

            if (Player.Hand.CalculateScore() == 21)
            {
                Console.WriteLine("Blackjack! Player wins immediately.");
                Player.Balance += (int)(Player.Bet * 2.5);
                Console.WriteLine($"Balance: {Player.Balance}");
                IsRoundComplete = true;
            }
        }

        public void DealerTurn()
        {
            Console.WriteLine($"Dealer reveals hidden card: {Dealer.Cards[1]}");

            if (Dealer.CalculateScore() == 21 && Player.InsuranceTaken)
            {
                Console.WriteLine("Dealer has Blackjack! Insurance pays 2:1.");
                Player.Balance += Player.Bet;
            }

            while (Dealer.CalculateScore() < 17)
            {
                Dealer.AddCard(Deck.Deal());
            }

            Console.WriteLine($"Dealer: {string.Join(", ", Dealer.Cards)} (Score: {Dealer.CalculateScore()})");
        }

        public void CompareHands()
        {
            int playerValue = Player.Hand.CalculateScore();
            int dealerValue = Dealer.CalculateScore();

            Console.WriteLine($"\nFinal Scores → Player: {playerValue}, Dealer: {dealerValue}");

            if (playerValue > 21)
                Console.WriteLine("Player busts! Lost bet.");
            else if (dealerValue > 21)
            {
                Console.WriteLine("Dealer busts! Player wins!");
                Player.Balance += Player.Bet * 2;
            }
            else if (playerValue > dealerValue)
            {
                Console.WriteLine("Player wins!");
                Player.Balance += Player.Bet * 2;
            }
            else if (playerValue == dealerValue)
            {
                Console.WriteLine("Push! Bet returned.");
                Player.Balance += Player.Bet;
            }
            else
                Console.WriteLine("Dealer wins! Lost bet.");

            Console.WriteLine($"Balance: {Player.Balance}");
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
                    string choice = (Console.ReadLine() ?? string.Empty).ToUpper();
                    if (choice == "R")
                    {
                        game.Player.Balance = 500;
                        Console.WriteLine("Balance reset to 500.");
                    }
                    else return;
                }

                game.Start();
                if (game.IsRoundComplete) goto Replay;

                bool playerTurn = true;
                while (playerTurn)
                {
                    Console.WriteLine("\nChoose action: (H)it, (S)tand, (D)ouble, (I)nsurance");
                    string choice = (Console.ReadLine() ?? string.Empty).ToUpper();

                    switch (choice)
                    {
                        case "H":
                            game.Player.Hit(game.Deck);
                            Console.WriteLine($"Player: {string.Join(", ", game.Player.Hand.Cards)} (Score: {game.Player.Hand.CalculateScore()})");
                            if (game.Player.Hand.CalculateScore() > 21)
                            {
                                Console.WriteLine("Player busts!");
                                playerTurn = false;
                                game.IsRoundComplete = true;
                            }
                            break;

                        case "S":
                            playerTurn = false;
                            break;

                        case "D":
                            game.Player.Double(game.Deck);
                            Console.WriteLine($"Player doubled. Hand: {string.Join(", ", game.Player.Hand.Cards)} (Score: {game.Player.Hand.CalculateScore()})");
                            if (game.Player.Hand.CalculateScore() > 21)
                            {
                                Console.WriteLine("Player busts!");
                                game.IsRoundComplete = true;
                            }
                            playerTurn = false;
                            break;

                        case "I":
                            game.Player.Insurance();
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
                }

            Replay:
                Console.WriteLine("\nDo you want to play again? (Y/N)");
                string replayChoice = (Console.ReadLine() ?? string.Empty).ToUpper();
                if (replayChoice != "Y") break;
            }
        }
    }
}











