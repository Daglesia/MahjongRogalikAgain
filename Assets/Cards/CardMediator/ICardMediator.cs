using System.Collections.Generic;
using UnityEngine;

public interface ICardMediator
{
    void NotifyCardSelected(Card card);
    void NotifyCardDeselected(Card card);
    void NotifyCardDiscardRequested(Card card);
    void NotifyComboAssembled(ComboType comboType, List<Card> selectedCards);
    void NotifyItemClicked(int slotIndex);
    void NotifyEndTurnRequested();
}
