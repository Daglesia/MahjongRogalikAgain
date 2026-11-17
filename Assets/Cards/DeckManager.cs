using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : CardColleague
{
    private Deck deck;

    public int GetCount() => deck.Count;
    public event Action<CardBase, int> CardDrawn;

    void Awake()
    {
        List<CardBase> runDeck = RunManager.Instance.State?.Deck;
        if (runDeck == null)
        {
            Debug.LogError("DeckManager: no run deck found. Start a run before entering combat.", this);
            runDeck = new List<CardBase>();
        }

        deck = new Deck(runDeck);
    }

    public CardBase DrawNextCard()
    {
        var card = (CardBase)deck.Draw();
        if (card == null) return null;          // deck empty, nothing to announce

        CardDrawn?.Invoke(card, deck.Count);
        return card;
    }
}
