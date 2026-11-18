using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckManager : CardColleague
{
    private Deck deck;
    private readonly List<CardBase> usedPile = new List<CardBase>();

    public int GetCount() => deck.Count;

    /// All cards that are still in the draw pile (not drawn into the hand and not used),
    /// sorted by card identity so the view never reveals the shuffled order.
    public IReadOnlyList<CardBase> GetRemainingCardsSorted()
    {
        if (deck == null) return Array.Empty<CardBase>();

        return deck.GetCards()
            .OfType<CardBase>()
            .OrderBy(CardSortKey)
            .ThenBy(card => card.name, StringComparer.Ordinal)
            .ToList();
    }

    private static int CardSortKey(CardBase card)
    {
        CardValue[] values = card.GetCardValues();
        return values != null && values.Length > 0 ? (int)values[0] : int.MaxValue;
    }

    public event Action<CardBase, int> CardDrawn;
    public event Action<int> DeckReshuffled;   // cards in the deck after reshuffling

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

    /// Played / discarded / thrown-away cards end up here until the next reshuffle.
    public void AddToUsedPile(CardBase card)
    {
        if (card != null) usedPile.Add(card);
    }

    public void AddToUsedPile(IEnumerable<CardBase> cards)
    {
        foreach (CardBase card in cards) AddToUsedPile(card);
    }

    /// Puts the used pile back into the deck and shuffles everything.
    public void ReshuffleUsedIntoDeck()
    {
        foreach (CardBase card in usedPile) deck.Add(card);
        usedPile.Clear();

        deck.Shuffle();
        DeckReshuffled?.Invoke(deck.Count);
    }
}