using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ComboType { None, Sequence, Triplet }

public class ComboManager : CardColleague
{
    public ComboType LastComboType { get; private set; } = ComboType.None;
    public CardValue[] LastComboValues { get; private set; } = Array.Empty<CardValue>();

    private List<ICard> cards = new List<ICard>();
    private readonly List<Card> selectedCards = new List<Card>();

    private static readonly int[][] PERMUTATIONS =
    {
        new[] { 0, 1, 2 }, new[] { 0, 2, 1 },
        new[] { 1, 0, 2 }, new[] { 1, 2, 0 },
        new[] { 2, 0, 1 }, new[] { 2, 1, 0 },
    };

    private bool TryGetSequence(out CardValue[] values)
    {
        int first = (int)CardValue.Bamboo1;
        int last = (int)CardValue.Bamboo6;

        for (int start = first; start + 2 <= last; start++)
        {
            foreach (int[] order in PERMUTATIONS)
            {
                if (cards[order[0]].GetCardValues().Contains((CardValue)start) &&
                    cards[order[1]].GetCardValues().Contains((CardValue)(start + 1)) &&
                    cards[order[2]].GetCardValues().Contains((CardValue)(start + 2)))
                {
                    values = new[] { (CardValue)start, (CardValue)(start + 1), (CardValue)(start + 2) };
                    return true;
                }
            }
        }

        values = Array.Empty<CardValue>();
        return false;
    }

    private bool TryGetTriplet(out CardValue result)
    {
        bool found = false;
        result = default;

        foreach (CardValue value in cards[0].GetCardValues())
        {
            if (!cards[1].GetCardValues().Contains(value) ||
                !cards[2].GetCardValues().Contains(value)) continue;

            if (!found || value >= CardValue.Earth)   // prefer an element if several match
            {
                result = value;
                found = true;
            }
        }

        return found;
    }

    private ComboType ResolveCombo(out CardValue[] values)
    {
        if (TryGetTriplet(out CardValue shared))
        {
            values = new[] { shared };
            return ComboType.Triplet;
        }

        if (TryGetSequence(out values)) return ComboType.Sequence;

        return ComboType.None;   // values is already empty from TryGetSequence
    }

    public void AddCardToCombo(Card card)
    {
        selectedCards.Add(card);
        cards.Add(card.GetCardBase());

        if (cards.Count < 3) return;

        ComboType comboType = ResolveCombo(out CardValue[] values);
        LastComboType = comboType;
        LastComboValues = values;   // must be set before notifying
        Mediator?.NotifyComboAssembled(comboType, selectedCards);

        selectedCards.Clear();
        cards.Clear();
    }

    public void RemoveCardFromCombo(Card card)
    {
        int index = selectedCards.IndexOf(card);
        if (index < 0) return;

        selectedCards.RemoveAt(index);
        cards.RemoveAt(index);   // cards and selectedCards are kept in the same order
    }

    public void ClearSelection()
    {
        selectedCards.Clear();
        cards.Clear();
    }
}
