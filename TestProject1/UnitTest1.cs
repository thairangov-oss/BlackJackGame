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
}

