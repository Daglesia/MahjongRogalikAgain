using System.Collections.Generic;
using UnityEngine;

public interface ICardMediator
{
    void NotifyCardSelected(Card card);
    void NotifyComboAssembled(bool isValidCombo, List<Card> selectedCards);
}
