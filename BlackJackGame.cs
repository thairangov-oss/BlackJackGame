using System;

namespace BlackjackGame
{
    public class BlackjackGame
    {
        public Deck Deck { get; private set; }
        public Player Player { get; set; }
        public Hand Dealer { get; private set; }

        // ✅ Constructor initializes all non-nullable properties
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
}

