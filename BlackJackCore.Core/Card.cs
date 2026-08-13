using System;

namespace BlackJackCore.Core
{
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
            string rankName = Rank switch
            {
                Rank.Jack => "Jack",
                Rank.Queen => "Queen",
                Rank.King => "King",
                Rank.Ace => "Ace",
                Rank.Two => "Two",
                Rank.Three => "Three",
                Rank.Four => "Four",
                Rank.Five => "Five",
                Rank.Six => "Six",
                Rank.Seven => "Seven",
                Rank.Eight => "Eight",
                Rank.Nine => "Nine",
                Rank.Ten => "Ten",
                _ => ((int)Rank).ToString()
            };

            return $"{rankName} of {Suit}";
        }

        public int Value()
        {
            return Rank switch
            {
                Rank.Ace => 11,
                Rank.King => 10,
                Rank.Queen => 10,
                Rank.Jack => 10,
                _ => (int)Rank
            };
        }
    }
}
