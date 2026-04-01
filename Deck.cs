public class Deck
{
    public List<Card> Cards { get; private set; }
    private Random rng = new Random();

    public Deck()
    {
        Cards = new List<Card>();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                Cards.Add(new Card(suit, rank));
            }
        }
    }

    public void Shuffle()
    {
        for (int i = Cards.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (Cards[i], Cards[j]) = (Cards[j], Cards[i]);
        }
    }

    public Card Deal()
    {
        Card card = Cards[0];
        Cards.RemoveAt(0);
        return card;
    }
}


