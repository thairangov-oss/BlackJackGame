using System.Collections.Generic;
using System.Linq;

namespace BlackJackCore.Core
{
    public class Hand
    {
        public List<Card> Cards { get; } = new List<Card>();

        public void AddCard(Card c) => Cards.Add(c);

        public int CalculateScore()
        {
            int total = Cards.Sum(c => c.Value());
            int aces = Cards.Count(c => c.Rank == Rank.Ace);

            while (total > 21 && aces > 0)
            {
                total -= 10;
                aces--;
            }

            return total;
        }

        public bool IsBlackjack()
        {
            return Cards.Count == 2 && CalculateScore() == 21;
        }
    }
}
