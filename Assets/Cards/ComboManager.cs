using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComboManager : CardColleague
{
    private List<ICard> cards = new List<ICard>();
    private readonly List<Card> selectedCards = new List<Card>();

    private static readonly int[][] PERMUTATIONS =
    {
        new[] { 0, 1, 2 }, new[] { 0, 2, 1 },
        new[] { 1, 0, 2 }, new[] { 1, 2, 0 },
        new[] { 2, 0, 1 }, new[] { 2, 1, 0 },
    };

    private bool IsSequence()
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
                    return true;
                }
            }
        }

        return false;
    }
    
    private bool IsTriplet()
    {
        foreach (CardValue value in cards[0].GetCardValues())
        {
            if (cards[1].GetCardValues().Contains(value) &&
                cards[2].GetCardValues().Contains(value))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsValidCombo() => IsSequence() || IsTriplet();

    public void AddCardToCombo(Card card)
    {
        selectedCards.Add(card);
        cards.Add(card.GetCardBase());

        if (cards.Count < 3) return;

        if (IsValidCombo())
        {
            foreach (Card c in selectedCards)
                c.FlyAndDisappear();
        }
        else
        {
            foreach (Card c in selectedCards)
                c.ResetCard();
        }

        selectedCards.Clear();
        cards.Clear();
    }
}
