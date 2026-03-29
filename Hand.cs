using System.Collections.Generic;

namespace BlackJackGame
{
    public class Hand
    {
        public List<Card> Cards { get; } = new List<Card>();

        public void AddCard(Card card) => Cards.Add(card);

        public int GetValue()
        {
            int total = 0;
            int aceCount = 0;

            foreach (var card in Cards)
            {
                total += (int)card.Rank;
                if (card.Rank == Rank.Ace) aceCount++;
            }

            while (total > 21 && aceCount > 0)
            {
                total -= 10;
                aceCount--;
            }

            return total;
        }
    }
}

