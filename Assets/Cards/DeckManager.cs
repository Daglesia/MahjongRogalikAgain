using UnityEngine;

public class DeckManager : CardColleague
{
    [SerializeField] private DeckGeneration deckGeneration;
    private Deck deck;

    public int GetCount() => deck.Count;

    void Start()
    {
        // todo somewhere down the line - different classes have different decks
        deck = deckGeneration.GenerateInitial();
    }

    public CardBase DrawNextCard()
    {
        return (CardBase)deck.Draw();
    }
}
