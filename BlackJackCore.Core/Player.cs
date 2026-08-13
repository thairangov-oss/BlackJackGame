using System;

namespace BlackJackCore.Core
{
    public class Player
    {
        public Hand Hand { get; } = new Hand();
        public int Balance { get; set; } = 500;
        public int Bet { get; private set; } = 0;
        public bool InsuranceTaken { get; private set; } = false;
        public int InsuranceAmount { get; private set; } = 0;

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
            if (Bet <= 0) throw new InvalidOperationException("No active bet for insurance");
            int insurance = Bet / 2;
            if (insurance <= 0) throw new InvalidOperationException("Insurance amount must be positive");
            if (Balance < insurance) throw new InvalidOperationException("Insufficient balance to take insurance");
            Balance -= insurance;
            InsuranceTaken = true;
            InsuranceAmount = insurance;
        }

        public int CalculateScore() => Hand.CalculateScore();

        public void ClearForNewRound()
        {
            Hand.Cards.Clear();
            Bet = 0;
            InsuranceTaken = false;
            InsuranceAmount = 0;
        }
    }
}
