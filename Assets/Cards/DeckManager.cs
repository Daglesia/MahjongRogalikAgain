using System;
using UnityEngine;

public class DeckManager : CardColleague
{
    [SerializeField] private DeckGeneration deckGeneration;
    private Deck deck;

    public int GetCount() => deck.Count;
    public event Action<CardBase, int> CardDrawn;

    void Start()
    {
        // todo somewhere down the line - different classes have different decks
        deck = deckGeneration.GenerateInitial();
        deck.Shuffle();
    }

    public CardBase DrawNextCard()
    {
        var card = (CardBase)deck.Draw();
        if (card == null) return null;          // deck empty, nothing to announce

        CardDrawn?.Invoke(card, deck.Count);
        return card;
    }
}
