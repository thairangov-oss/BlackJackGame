using BlackjackGame;
using Xunit;

namespace TestProject1
{
    public class HandTests
    {
        [Fact]
        public void Hand_WithMultipleAces_AdjustsScoreCorrectly()
        {
            var hand = new Hand();
            hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
            hand.AddCard(new Card(Suit.Spades, Rank.Ace));
            hand.AddCard(new Card(Suit.Clubs, Rank.Nine));

            int score = hand.CalculateScore();

            Assert.Equal(21, score);
        }

        [Fact]
        public void Hand_WithFaceCards_CountsAsTen()
        {
            var hand = new Hand();
            hand.AddCard(new Card(Suit.Hearts, Rank.Jack));
            hand.AddCard(new Card(Suit.Spades, Rank.Queen));
            hand.AddCard(new Card(Suit.Clubs, Rank.King));

            int score = hand.CalculateScore();

            Assert.Equal(30, score);
        }

        [Fact]
        public void Hand_BustCondition_Over21()
        {
            var hand = new Hand();
            hand.AddCard(new Card(Suit.Hearts, Rank.King));
            hand.AddCard(new Card(Suit.Spades, Rank.Queen));
            hand.AddCard(new Card(Suit.Clubs, Rank.Two));

            int score = hand.CalculateScore();

            Assert.True(score > 21);
        }

        [Theory]
        [InlineData(Rank.Ace, Rank.Ace, Rank.Nine, 21)]
        [InlineData(Rank.Ace, Rank.Ace, Rank.Ace, 13)]
        [InlineData(Rank.Ace, Rank.King, Rank.Nine, 20)]
        public void Hand_AceCombinations_CalculateCorrectScore(Rank r1, Rank r2, Rank r3, int expected)
        {
            var hand = new Hand();
            hand.AddCard(new Card(Suit.Hearts, r1));
            hand.AddCard(new Card(Suit.Spades, r2));
            hand.AddCard(new Card(Suit.Clubs, r3));

            Assert.Equal(expected, hand.CalculateScore());
        }
    }

    public class CardValueHelperTests
    {
        [Theory]
        [InlineData(Rank.Two, 2)]
        [InlineData(Rank.Three, 3)]
        [InlineData(Rank.Four, 4)]
        [InlineData(Rank.Five, 5)]
        [InlineData(Rank.Six, 6)]
        [InlineData(Rank.Seven, 7)]
        [InlineData(Rank.Eight, 8)]
        [InlineData(Rank.Nine, 9)]
        [InlineData(Rank.Ten, 10)]
        [InlineData(Rank.Jack, 10)]
        [InlineData(Rank.Queen, 10)]
        [InlineData(Rank.King, 10)]
        [InlineData(Rank.Ace, 11)]
        public void GetCardValue_ReturnsCorrectValue(Rank rank, int expected)
        {
            Assert.Equal(expected, CardValueHelper.GetCardValue(rank));
        }
    }

    public class DeckTests
    {
        [Fact]
        public void Deck_Has52CardsInitially()
        {
            var deck = new Deck();
            Assert.Equal(52, deck.Cards.Count);
        }

        [Fact]
        public void Deck_DealReducesCardCount()
        {
            var deck = new Deck();
            var card = deck.Deal();
            Assert.Equal(51, deck.Cards.Count);
            Assert.NotNull(card);
        }
    }
}


