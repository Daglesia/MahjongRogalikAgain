using System.Collections.Generic;

public class Deck
{
    private readonly List<ICard> cards;

    public int Count => cards.Count;

    public Deck(IEnumerable<ICard> cards)
    {
        this.cards = new List<ICard>(cards);
    }

    public void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }

    public ICard Draw()
    {
        if (cards.Count == 0) return null;
        var card = cards[^1];
        cards.RemoveAt(cards.Count - 1);
        return card;
    }

    public void Add(ICard card) => cards.Add(card);
}