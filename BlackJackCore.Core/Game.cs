using System;

namespace BlackJackCore.Core
{
    public class Game
    {
        public Deck Deck { get; private set; } = new Deck();
        public Player Player { get; } = new Player();
        public Player Dealer { get; } = new Player();
        public bool IsRoundComplete { get; private set; } = false;
        public string OutcomeMessage { get; private set; } = string.Empty;

        public Game()
        {
            Deck.Shuffle();
        }

        public void ForceNewDeck(Deck deck)
        {
            Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        }

        public void Start(int bet)
        {
            IsRoundComplete = false;
            OutcomeMessage = string.Empty;
            Player.ClearForNewRound();
            Dealer.ClearForNewRound();

            Player.PlaceBet(bet);

            Player.Hand.AddCard(Deck.Deal());
            Dealer.Hand.AddCard(Deck.Deal());
            Player.Hand.AddCard(Deck.Deal());
            Dealer.Hand.AddCard(Deck.Deal());

            if (Player.Hand.IsBlackjack())
            {
                if (Dealer.Hand.IsBlackjack())
                {
                    Player.Balance += Player.Bet;
                    OutcomeMessage = "Push (both blackjack)";
                }
                else
                {
                    int betAmount = Player.Bet;
                    int payout = betAmount + (betAmount * 3 / 2);
                    Player.Balance += payout;
                    OutcomeMessage = "Blackjack! Player wins";
                }

                IsRoundComplete = true;
            }
        }

        public void Hit()
        {
            if (IsRoundComplete) throw new InvalidOperationException("Round already complete");
            Player.Hand.AddCard(Deck.Deal());
            if (Player.CalculateScore() > 21)
            {
                OutcomeMessage = "Player busts";
                IsRoundComplete = true;
            }
            else if (Player.CalculateScore() == 21)
            {
                Stand();
            }
        }

        public void Stand()
        {
            if (IsRoundComplete) throw new InvalidOperationException("Round already complete");
            DealerTurn();
            CompareHands();
            IsRoundComplete = true;
        }

        public void Double()
        {
            if (IsRoundComplete) throw new InvalidOperationException("Round already complete");
            // Only allow doubling on initial two-card hand (player's first decision)
            if (Player.Hand.Cards.Count != 2)
                throw new InvalidOperationException("Double is only allowed on the initial two-card hand");
            Player.Double(Deck);
            if (Player.CalculateScore() > 21)
            {
                OutcomeMessage = "Player busts after double";
                IsRoundComplete = true;
                return;
            }
            DealerTurn();
            CompareHands();
            IsRoundComplete = true;
        }

        public void TakeInsurance()
        {
            if (IsRoundComplete) throw new InvalidOperationException("Round already complete");
            // Insurance only allowed when dealer's visible card is an Ace
            var visible = Dealer.Hand.Cards.Count > 0 ? Dealer.Hand.Cards[0] : null;
            if (visible == null || visible.Rank != Rank.Ace)
                throw new InvalidOperationException("Insurance is only allowed when dealer's visible card is an Ace");
            Player.TakeInsurance();
        }

        public void DealerTurn()
        {
            while (Dealer.CalculateScore() < 17)
            {
                Dealer.Hand.AddCard(Deck.Deal());
            }
        }

        public void CompareHands()
        {
            int playerScore = Player.CalculateScore();
            int dealerScore = Dealer.CalculateScore();
            int bet = Player.Bet;

            // Handle dealer blackjack (and insurance) first
            if (Dealer.Hand.IsBlackjack())
            {
                if (Player.InsuranceTaken)
                {
                    // Insurance pays 2:1 on the insurance amount
                    Player.Balance += Player.InsuranceAmount * 2;
                }

                if (Player.Hand.IsBlackjack())
                {
                    // Push - return bet
                    Player.Balance += bet;
                    OutcomeMessage = "Push.";
                    return;
                }

                // Dealer blackjack and player doesn't have blackjack -> dealer wins
                OutcomeMessage = "Dealer has blackjack. Dealer wins.";
                return;
            }

            if (playerScore > 21)
            {
                OutcomeMessage = "Player busts. Dealer wins.";
                return;
            }

            if (dealerScore > 21)
            {
                Player.Balance += bet * 2;
                OutcomeMessage = "Dealer busts. Player wins.";
                return;
            }

            if (playerScore > dealerScore)
            {
                Player.Balance += bet * 2;
                OutcomeMessage = "Player wins.";
            }
            else if (playerScore < dealerScore)
            {
                OutcomeMessage = "Dealer wins.";
            }
            else
            {
                Player.Balance += bet;
                OutcomeMessage = "Push.";
            }
        }
    }
}
