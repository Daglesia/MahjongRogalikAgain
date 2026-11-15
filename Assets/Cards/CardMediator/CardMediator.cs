using System.Collections.Generic;
using UnityEngine;

public class CardMediator : MonoBehaviour, ICardMediator
{
    [SerializeField] private ComboManager _comboManager;
    [SerializeField] private DeckManager _deckManager;
    [SerializeField] private DiscardManager _discardManager;
    [SerializeField] private HandManager _handManager;
    [SerializeField] private InventoryManager _inventoryManager;

    private void Awake()
    {
        _comboManager.SetMediator(this);
        _deckManager.SetMediator(this);
        _discardManager.SetMediator(this);
        _handManager.SetMediator(this);
        _inventoryManager.SetMediator(this);
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

    void ICardMediator.NotifyItemClicked(int slotIndex)
{
    ItemBase item = _inventoryManager.GetItem(slotIndex);
    if (item == null) return;

    var context = new ItemContext(_deckManager, _discardManager, _handManager);
    if (item.Use(context) && item.Consumable)
        _inventoryManager.RemoveItem(slotIndex);
}
}