using System.Collections.Generic;
using UnityEngine;

public class CardMediator : MonoBehaviour, ICardMediator
{
    [SerializeField] private ComboManager _comboManager;
    [SerializeField] private DeckManager _deckManager;
    [SerializeField] private DiscardManager _discardManager;
    [SerializeField] private HandManager _handManager;
    [SerializeField] private InventoryManager _inventoryManager;
    [SerializeField] private AudioManager _audioManager;
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private TurnManager _turnManager;

    private TargetedItemBase pendingItem;
    private int pendingSlot = -1;

    private void Awake()
    {
        _comboManager.SetMediator(this);
        _deckManager.SetMediator(this);
        _discardManager.SetMediator(this);
        _handManager.SetMediator(this);
        _inventoryManager.SetMediator(this);
        _audioManager.SetMediator(this);
        _scoreManager.SetMediator(this);
        _turnManager.SetMediator(this);
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

    void ICardMediator.NotifyCardDeselected(Card card)
    {
        if (pendingItem != null) return;

        _comboManager.RemoveCardFromCombo(card);
        _handManager.LowerCards(new List<Card> { card });
    }

    void ICardMediator.NotifyComboAssembled(ComboType comboType, List<Card> selectedCards)
    {
        if (comboType == ComboType.None)
        {
            _handManager.LowerCards(selectedCards);
            return;
        }

        _audioManager.PlaySound(comboType == ComboType.Triplet
            ? SoundType.TripletPlayed
            : SoundType.SequencePlayed);

        _scoreManager.ScoreCombo(comboType, selectedCards);

        SendToUsedPile(selectedCards);
        _handManager.RemoveAndReplace(selectedCards, _deckManager.DrawNextCard);
    }

    void ICardMediator.NotifyCardDiscardRequested(Card card)
    {
        if (pendingItem != null) { CancelTargeting(); return; }   // right click cancels targeting

        if (card.Definition.GetIsClicked()) return;
        if (!_discardManager.TryUseDiscard()) return;

        var discarded = new List<Card> { card };
        SendToUsedPile(discarded);
        _handManager.RemoveAndReplace(discarded, _deckManager.DrawNextCard);
    }

    void ICardMediator.NotifyEndTurnRequested()
    {
        if (_handManager.IsBusy) return;   // already redrawing

        CancelTargeting();
        _comboManager.ClearSelection();

        if (_turnManager.IsLastTurn)
        {
            EndCombat();
            return;
        }

        _turnManager.TryAdvanceTurn();
        _handManager.ReplaceHand(_deckManager.DrawNextCard, removed =>
        {
            _deckManager.AddToUsedPile(removed);
            _deckManager.ReshuffleUsedIntoDeck();
        });
    }

    void ICardMediator.NotifyItemClicked(int slotIndex)
    {
        ItemBase item = _inventoryManager.GetItem(slotIndex);
        if (item == null) return;

        if (item is TargetedItemBase targeted)
        {
            if (pendingSlot == slotIndex) CancelTargeting();
            else BeginTargeting(targeted, slotIndex);
            return;
        }

        CancelTargeting();
        var context = new ItemContext(_deckManager, _discardManager, _handManager);
        if (item.Use(context) && item.Consumable)
            _inventoryManager.RemoveItem(slotIndex);
    }

    // ---------- Combat flow ----------

    private void EndCombat()
    {
        // todo: win/lose check against a score target, rewards, etc.
        SceneLoader.Instance.ReturnToMap();
    }

    private void SendToUsedPile(IEnumerable<Card> cards)
    {
        foreach (Card card in cards)
            _deckManager.AddToUsedPile(card.GetCardBase());
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
        card.Deselect();

        if (!pendingItem.CanTarget(card)) return;

        var context = new ItemContext(_deckManager, _discardManager, _handManager);
        if (!pendingItem.UseOnCard(context, card)) return;

        if (pendingItem.Consumable)
            _inventoryManager.RemoveItem(pendingSlot);

        CancelTargeting();
    }
}