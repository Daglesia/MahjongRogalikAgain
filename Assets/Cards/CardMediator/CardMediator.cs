using System.Collections.Generic;
using UnityEngine;

public class CardMediator : MonoBehaviour, ICardMediator
{
    [SerializeField] private ComboManager _comboManager;
    [SerializeField] private DeckManager _deckManager;
    [SerializeField] private DiscardManager _discardManager;
    [SerializeField] private HandManager _handManager;

    private void Awake()
    {
        _comboManager.SetMediator(this);
        _deckManager.SetMediator(this);
        _discardManager.SetMediator(this);
        _handManager.SetMediator(this);
    }

    private void Start() => _handManager.DealStartingHand(_deckManager.DrawNextCard);

    void ICardMediator.NotifyCardSelected(Card card)
    {
        _handManager.LiftCard(card);
        _comboManager.AddCardToCombo(card);
    }

    void ICardMediator.NotifyComboAssembled(bool isValidCombo, List<Card> selectedCards)
    {
        if (isValidCombo)
            _handManager.RemoveAndReplace(selectedCards, _deckManager.DrawNextCard);
        else
            _handManager.LowerCards(selectedCards);
    }

    void ICardMediator.NotifyCardDiscardRequested(Card card)
    {
        if (card.Definition.GetIsClicked()) return;
        if (!_discardManager.TryUseDiscard()) return;   // no discards left

        _handManager.RemoveAndReplace(new List<Card> { card }, _deckManager.DrawNextCard);
    }
}