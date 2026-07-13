using System;
using System.Collections.Generic;

namespace BlackJackCore.Core
{
    public class Deck
    {
        public List<Card> Cards { get; } = new List<Card>();
        private static readonly Random _rnd = new Random();

        public Deck()
        {
            Populate();
        }

        private void Populate()
        {
            Cards.Clear();
            foreach (Suit s in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank r in Enum.GetValues(typeof(Rank)))
                {
                    Cards.Add(new Card(s, r));
                }
            }
        }

        public void Shuffle()
        {
            int n = Cards.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = _rnd.Next(i + 1);
                var tmp = Cards[i];
                Cards[i] = Cards[j];
                Cards[j] = tmp;
            }
        }

        public Card Deal()
        {
            if (Cards.Count == 0) throw new InvalidOperationException("Deck is empty");
            var card = Cards[0];
            Cards.RemoveAt(0);
            return card;
        }
    }
}
