using System;

namespace BlackJack1
{
    public class Player
    {
        public Hand Hand { get; } = new Hand();
        public int Balance { get; set; } = 500;
        public int Bet { get; private set; } = 0;
        public bool InsuranceTaken { get; private set; } = false;

        public void PlaceBet(int amount)
        {
            if (amount <= 0) throw new ArgumentException("Bet must be positive");
            if (amount > Balance) throw new InvalidOperationException("Insufficient balance");
            Bet = amount;
            Balance -= amount;
        }

        public void Double(Deck deck)
        {
            if (Bet <= 0) throw new InvalidOperationException("No active bet to double");
            if (Balance < Bet) throw new InvalidOperationException("Insufficient balance to double");
            Balance -= Bet;
            Bet *= 2;
            Hand.AddCard(deck.Deal());
        }

        public void TakeInsurance()
        {
            InsuranceTaken = true;
            // Minimal implementation: flag only
        }

        public int CalculateScore() => Hand.CalculateScore();

        public void ClearForNewRound()
        {
            Hand.Cards.Clear();
            Bet = 0;
            InsuranceTaken = false;
        }
    }
}
