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
        CombatContext ctx = SceneLoader.Instance.CurrentCombat;

        List<CardBase> source = ctx != null
            ? new List<CardBase>(ctx.Deck)
            : RunManager.Instance.State?.Deck;   // fallback: playing CombatScene directly in the editor

        if (source == null)
        {
            Debug.LogError("DeckManager: no deck available. Start a run before entering combat.", this);
            source = new List<CardBase>();
        }

        deck = new Deck(source);
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
