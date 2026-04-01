using System;
using System.Collections.Generic;

namespace BlackjackGame
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }

    public enum Rank
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack = 10, Queen = 10, King = 10, Ace = 11
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

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }

    public class Deck
    {
        public List<Card> Cards { get; private set; }
        private Random rng = new Random();

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

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public int CalculateScore()
        {
            int total = 0;
            int aceCount = 0;

            foreach (var card in Cards)
            {
                total += (int)card.Rank;
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

        public void Hit(Deck deck)
        {
            Hand.AddCard(deck.Deal());
        }

        public void Double(Deck deck)
        {
            if (Balance >= Bet)
            {
                Balance -= Bet;
                Bet *= 2;
                Hit(deck);
            }
            else Console.WriteLine("Not enough balance to double.");
        }

        public void Insurance()
        {
            if (Balance >= Bet / 2)
            {
                Balance -= Bet / 2;
                Console.WriteLine("Insurance taken.");
            }
            else Console.WriteLine("Not enough balance for insurance.");
        }
    }

    public class BlackjackGame
    {
        public Deck Deck { get; private set; }
        public Player Player { get; set; }
        public Hand Dealer { get; private set; }

        public BlackjackGame()
        {
            Deck = new Deck();
            Player = new Player();
            Dealer = new Hand();
        }

        public void Start()
        {
            Deck = new Deck();
            Deck.Shuffle();

            Player.Hand.Cards.Clear();
            Dealer = new Hand();

            Player.PlaceBet();

            Player.Hand.AddCard(Deck.Deal());
            Dealer.AddCard(Deck.Deal());
            Player.Hand.AddCard(Deck.Deal());
            Dealer.AddCard(Deck.Deal());

            Console.WriteLine($"Player: {string.Join(", ", Player.Hand.Cards)} (Score: {Player.Hand.CalculateScore()})");
            Console.WriteLine($"Dealer: {Dealer.Cards[0]} and [Hidden]");

            // Check for immediate Blackjack
            if (Player.Hand.CalculateScore() == 21)
            {
                Console.WriteLine("Blackjack! Player wins immediately.");
                Player.Balance += (int)(Player.Bet * 2.5); // 3:2 payout
                Console.WriteLine($"Balance: {Player.Balance}");
                return;
            }
        }

        public void DealerTurn()
        {
            Console.WriteLine($"Dealer reveals hidden card: {Dealer.Cards[1]}");
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
            Console.WriteLine("Verifying deck contents...");
            Deck verifyDeck = new Deck();
            Console.WriteLine("Cards have been verified");
            Console.WriteLine($"Total cards: {verifyDeck.Cards.Count}\n");

            BlackjackGame game = new BlackjackGame();

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
                            }
                            break;

                        case "S":
                            playerTurn = false;
                            break;

                        case "D":
                            game.Player.Double(game.Deck);
                            Console.WriteLine($"Player doubled. Hand: {string.Join(", ", game.Player.Hand.Cards)} (Score: {game.Player.Hand.CalculateScore()})");
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

                game.DealerTurn();
                game.CompareHands();


                Console.WriteLine("\nDo you want to play again? (Y/N)");
                string replayChoice = (Console.ReadLine() ?? string.Empty).ToUpper();
                if (replayChoice != "Y") break;
            }
        }
    }
}
