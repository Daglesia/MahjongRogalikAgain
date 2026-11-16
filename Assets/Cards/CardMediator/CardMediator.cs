using System.Collections.Generic;
using UnityEngine;

public class CardMediator : MonoBehaviour, ICardMediator
{
    [SerializeField] private ComboManager _comboManager;
    [SerializeField] private DeckManager _deckManager;
    [SerializeField] private DiscardManager _discardManager;
    [SerializeField] private HandManager _handManager;
    [SerializeField] private InventoryManager _inventoryManager;

    private TargetedItemBase pendingItem;
    private int pendingSlot = -1;

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
        if (pendingItem != null)
        {
            ResolveItemTarget(card);
            return;
        }

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
        if (pendingItem != null) { CancelTargeting(); return; }   // right click cancels targeting

        if (card.Definition.GetIsClicked()) return;
        if (!_discardManager.TryUseDiscard()) return;

        _handManager.RemoveAndReplace(new List<Card> { card }, _deckManager.DrawNextCard);
    }

    void ICardMediator.NotifyItemClicked(int slotIndex)
    {
        ItemBase item = _inventoryManager.GetItem(slotIndex);
        if (item == null) return;

        if (item is TargetedItemBase targeted)
        {
            // clicking the armed item again cancels it
            if (pendingSlot == slotIndex) CancelTargeting();
            else BeginTargeting(targeted, slotIndex);
            return;
        }

        CancelTargeting();
        var context = new ItemContext(_deckManager, _discardManager, _handManager);
        if (item.Use(context) && item.Consumable)
            _inventoryManager.RemoveItem(slotIndex);
    }

    // ---------- Item targeting ----------

    private void BeginTargeting(TargetedItemBase item, int slot)
    {
        pendingItem = item;
        pendingSlot = slot;
        // todo: highlight valid targets / the armed slot
    }

    private void CancelTargeting()
    {
        pendingItem = null;
        pendingSlot = -1;
    }

    private void ResolveItemTarget(Card card)
    {
        // Card.OnMouseDown already flagged it as clicked; targeting must not count as a combo selection
        card.Deselect();

        if (!pendingItem.CanTarget(card)) return;   // invalid target: stay armed, player can pick another

        var context = new ItemContext(_deckManager, _discardManager, _handManager);
        if (!pendingItem.UseOnCard(context, card)) return;

        if (pendingItem.Consumable)
            _inventoryManager.RemoveItem(pendingSlot);

        CancelTargeting();
    }
}