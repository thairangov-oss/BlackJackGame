using System;

namespace BlackJackGame
{
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
                Console.WriteLine("Insurance taken.");
            }
            else Console.WriteLine("Not enough balance for insurance.");
        }
    }
}

