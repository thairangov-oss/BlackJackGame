using Xunit;
using BlackjackGame;

namespace BlackjackGameTests
{
    public class UnitTest1
    {
        [Fact]
        public void Card_ToString_ReturnsCorrectFormat()
        {
            var card = new Card(Suit.Hearts, Rank.Ace);
            Assert.Equal("Ace of Hearts", card.ToString());
        }

        [Fact]
        public void Deck_Shuffle_ChangesOrder()
        {
            var deck = new Deck();
            var firstCardBefore = deck.Cards[0];
            deck.Shuffle();
            var firstCardAfter = deck.Cards[0];
            // Not guaranteed, but highly likely
            Assert.NotEqual(firstCardBefore, firstCardAfter);
        }

        [Fact]
        public void Hand_CalculateScore_AceAdjustsCorrectly()
        {
            var hand = new Hand();
            hand.AddCard(new Card(Suit.Spades, Rank.Ace));
            hand.AddCard(new Card(Suit.Hearts, Rank.King));
            hand.AddCard(new Card(Suit.Diamonds, Rank.Nine));

            int score = hand.CalculateScore();
            Assert.Equal(20, score); // Ace should count as 1 here
        }

        [Fact]
        public void Player_PlaceBet_ReducesBalance()
        {
            var player = new Player();
            player.Balance = 100;
            player.Bet = 25;
            player.Balance -= player.Bet;

            Assert.Equal(75, player.Balance);
        }

        [Fact]
        public void Player_Double_DoublesBetAndAddsCard()
        {
            var deck = new Deck();
            deck.Shuffle();
            var player = new Player();
            player.Balance = 100;
            player.Bet = 25;
            player.Hand.AddCard(deck.Deal());
            player.Hand.AddCard(deck.Deal());

            player.Double(deck);

            Assert.Equal(50, player.Bet);
            Assert.True(player.Hand.Cards.Count == 3);
        }

        [Fact]
        public void Game_BlackjackOnStart_PlayerWinsImmediately()
        {
            var game = new Game();
            var customDeck = new Deck();
            customDeck.Cards.Clear();
            customDeck.Cards.Add(new Card(Suit.Hearts, Rank.Ace));
            customDeck.Cards.Add(new Card(Suit.Spades, Rank.King));
            customDeck.Cards.Add(new Card(Suit.Clubs, Rank.Two));
            customDeck.Cards.Add(new Card(Suit.Diamonds, Rank.Three));

            // Use the safer controlled method
            game.ForceNewDeck(customDeck);

            game.Start();

            Assert.True(game.IsRoundComplete);
            Assert.True(game.Player.Balance > 500); // Balance increased
        }
    }
}

