using UnityEngine;

public class CardMediator : MonoBehaviour, ICardMediator
{
    [SerializeField] private ComboManager _comboManager;
    [SerializeField] private Vector2 comboTarget = new Vector2(-7f, -3.11f);

    private void Awake()
    {
        _comboManager.SetMediator(this);
    } 

    void ICardMediator.NotifyCardSelected(Card card)
    {
        Debug.Log("yay");
        _comboManager.AddCardToCombo(card);
    }

    void ICardMediator.NotifyComboAssembled(bool isValidCombo, Card[] selectedCards)
    {
        throw new System.NotImplementedException();
    }
}