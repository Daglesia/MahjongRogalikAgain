using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    private List<ICard> cards = new List<ICard>();

    private bool IsValidCombo()
    {
        // todo incomplete
        if (cards[0].GetCardValues()[0] == CardValue.Bamboo1)
        {
            return true;
        }

        return false;
    }

    public void AddCardToCombo(ICard card) {
        cards.Add(card);
        if(cards.Count == 3)
        {
            if (IsValidCombo())
            {
                
            }
        }
    }
}
