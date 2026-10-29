using UnityEngine;

public interface ICardMediator
{
    void NotifyCardSelected(Card card);
    void NotifyComboAssembled(bool isValidCombo, Card[] selectedCards);
}
