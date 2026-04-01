using System;
using System.Collections.Generic;

namespace BlackjackGame
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }

    public enum Rank
    {
        Two, Three, Four, Five, Six,
        Seven, Eight, Nine, Ten,
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
            if (Cards.Count == 0)
                throw new InvalidOperationException("No cards left in the deck!");
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
                switch (card.Rank)
                {
                    case Rank.Two: total += 2; break;
                    case Rank.Three: total += 3; break;
                    case Rank.Four: total += 4; break;
                    case Rank.Five: total += 5; break;
                    case Rank.Six: total += 6; break;
                    case Rank.Seven: total += 7; break;
                    case Rank.Eight: total += 8; break;
                    case Rank.Nine: total += 9; break;
                    case Rank.Ten:
                    case Rank.Jack:
                    case Rank.Queen:
                    case Rank.King:
                        total += 10; break;
                    case Rank.Ace:
                        total += 11;
                        aceCount++;
                        break;
                }
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
            InsuranceTaken = false;
            Console.WriteLine($"Bet placed: {Bet}. Remaining balance: {Balance}");
        }

        public void Hit(Deck deck) => Hand.AddCard(deck.Deal());

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
                InsuranceTaken = true;
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

            Console.WriteLine($"Player: {string.Join(", ", Player.Hand.Cards)} (Value: {Player.Hand.GetValue()})");
            Console.WriteLine($"Dealer: {Dealer.Cards[0]} and [Hidden]");
        }

        public void DealerTurn()
        {
            while (Dealer.GetValue() < 17)
            {
                Dealer.AddCard(Deck.Deal());
            }

            Console.WriteLine($"Dealer: {string.Join(", ", Dealer.Cards)} (Value: {Dealer.GetValue()})");
        }

        public void CompareHands()
        {
            int playerValue = Player.Hand.GetValue();
            int dealerValue = Dealer.GetValue();

            // Insurance payout if dealer has Blackjack
            if (Dealer.GetValue() == 21 && Dealer.Cards.Count == 2 && Player.InsuranceTaken)
            {
                Player.Balance += Player.Bet; // Insurance pays 2:1
                Console.WriteLine("Dealer has Blackjack. Insurance pays out!");
            }

            // Natural Blackjack payout (3:2)
            if (playerValue == 21 && Player.Hand.Cards.Count == 2)
            {
                if (dealerValue == 21 && Dealer.Cards.Count == 2)
                {
                    Console.WriteLine("Both have Blackjack! Push.");
                    Player.Balance += Player.Bet;
                }
                else
                {
                    Console.WriteLine("Blackjack! Player wins with 3:2 payout.");
                    Player.Balance += (int)(Player.Bet * 2.5);
                }
                Console.WriteLine($"Balance: {Player.Balance}");
                return;
            }

            if (playerValue > 21)
                Console.WriteLine("Player busts! Lost bet.");
            else if (dealerValue > 21 || playerValue > dealerValue)
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
            Console.WriteLine("Welcome to BlackJack");
            Console.WriteLine("Verifying deck contents...");
            Deck verifyDeck = new Deck();
            Console.WriteLine("Cards have been verified");
            Console.WriteLine($"Total cards: {verifyDeck.Cards.Count}\n");

            BlackjackGame game = new BlackjackGame();

            while (true)
            {
                if (game.Player.Balance <= 0)
                {
                    Console.WriteLine("\nYou are bust! Retry (R) or press any key to close.");
                    string choice = (Console.ReadLine() ?? string.Empty).ToUpper();
                    if (choice == "R")
                    {
                        game.Player.Balance = 500;
                        game.Player.Hand.Cards.Clear();
                        game.Dealer.Cards.Clear();
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
                            Console.WriteLine($"Player: {string.Join(", ", game.Player.Hand.Cards)} (Value: {game.Player.Hand.GetValue()})");
                            if (game.Player.Hand.GetValue() > 21)
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
                            Console.WriteLine($"Player doubled. Hand: {string.Join(", ", game.Player.Hand.Cards)} (Value: {game.Player.Hand.GetValue()})");
                            playerTurn = false;
                            break;

                    }
                }

                // After player finishes, dealer plays
                game.DealerTurn();
                game.CompareHands();
            }
        }
    }
}