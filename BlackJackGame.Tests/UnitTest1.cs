using BlackJackCore.Core;
using System;
using Xunit;

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
            Assert.Equal(20, score);
        }

        [Fact]
        public void Player_PlaceBet_ReducesBalance()
        {
            var player = new Player();
            player.PlaceBet(50);
            Assert.Equal(450, player.Balance);
            Assert.Equal(50, player.Bet);
        }

        [Fact]
        public void Player_Double_DoublesBetAndAddsCard()
        {
            var deck = new Deck();
            deck.Shuffle();
            var player = new Player();
            player.PlaceBet(25);
            player.Hand.AddCard(deck.Deal());
            player.Hand.AddCard(deck.Deal());

            player.Double(deck);

            Assert.Equal(50, player.Bet);
            Assert.Equal(450, player.Balance);
            Assert.Equal(3, player.Hand.Cards.Count);
        }

        [Fact]
        public void Player_TakeInsurance_ReducesBalanceAndSetsFlag()
        {
            var player = new Player();
            player.PlaceBet(100);
            // TakeInsurance now deducts Bet/2 from balance
            // initial balance 500 -> after bet 400 -> after insurance 350
            player.TakeInsurance();

            Assert.True(player.InsuranceTaken);
            Assert.Equal(350, player.Balance);
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

            game.ForceNewDeck(customDeck);
            game.Start(50);

            Assert.True(game.IsRoundComplete);
            Assert.True(game.Player.Balance > 500);
        }

        [Fact]
        public void DealerTurn_StopsAtSeventeenOrMore()
        {
            var game = new Game();
            var customDeck = new Deck();
            customDeck.Cards.Clear();
            customDeck.Cards.Add(new Card(Suit.Hearts, Rank.Ten));
            customDeck.Cards.Add(new Card(Suit.Spades, Rank.Six));
            customDeck.Cards.Add(new Card(Suit.Clubs, Rank.Five));
            customDeck.Cards.Add(new Card(Suit.Diamonds, Rank.Four));

            game.ForceNewDeck(customDeck);
            game.Start(25);
            game.DealerTurn();

            Assert.True(game.Dealer.CalculateScore() >= 17);
        }

        [Fact]
        public void CompareHands_PlayerWinsAgainstDealer()
        {
            var game = new Game();
            var deck = new Deck();
            deck.Cards.Clear();

            deck.Cards.Add(new Card(Suit.Hearts, Rank.King));
            deck.Cards.Add(new Card(Suit.Spades, Rank.Nine));
            deck.Cards.Add(new Card(Suit.Clubs, Rank.Queen));
            deck.Cards.Add(new Card(Suit.Diamonds, Rank.Eight));

            game.ForceNewDeck(deck);
            game.Start(50);
            game.CompareHands();

            Assert.True(game.Player.Balance > 500);
        }
    }
}
