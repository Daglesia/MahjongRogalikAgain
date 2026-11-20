using System.Collections;
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
    [SerializeField] private ComboStarManager _comboStarManager;
    [SerializeField] private TurnManager _turnManager;
    [SerializeField] private EncounterManager _encounterManager;
    [SerializeField] private TargetingArrow _targetingArrow;

    [Tooltip("How long the VICTORY / DEFEAT text stays up before returning to the map")]
    [SerializeField, Min(0f)] private float _resultDelay = 1.5f;

    private TargetedItemBase pendingItem;
    private int pendingSlot = -1;

    private bool _combatOver;
    private bool _resolvingLastTurn;   // last turn ended, waiting for the score queue to drain

    // The deck overlay is a Canvas, so it cannot block world-space OnMouseDown card clicks.
    private bool InputLocked => _combatOver || _resolvingLastTurn || DeckView.AnyOpen || PauseView.AnyOpen;

    private void Awake()
    {
        _comboManager.SetMediator(this);
        _deckManager.SetMediator(this);
        _discardManager.SetMediator(this);
        _handManager.SetMediator(this);
        _inventoryManager.SetMediator(this);
        _audioManager.SetMediator(this);
        _scoreManager.SetMediator(this);
        _comboStarManager.SetMediator(this);
        _turnManager.SetMediator(this);
        _encounterManager.SetMediator(this);
    }

    private void Start() => _handManager.DealStartingHand(_deckManager.DrawNextCard);

    void ICardMediator.NotifyCardSelected(Card card)
    {
        if (InputLocked)
        {
            card.Deselect();   // Card.OnMouseDown already flagged it as clicked
            return;
        }

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
        if (InputLocked || pendingItem != null) return;

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
        _comboStarManager.RegisterCombo(comboType, _comboManager.LastComboValues);

        SendToUsedPile(selectedCards);
        _handManager.RemoveAndReplace(selectedCards, _deckManager.DrawNextCard);
    }

    void ICardMediator.NotifyCardDiscardRequested(Card card)
    {
        if (InputLocked) return;
        if (pendingItem != null) { CancelTargeting(); return; }   // right click cancels targeting

        if (card.Definition.GetIsClicked()) return;
        if (!_discardManager.TryUseDiscard()) return;

        var discarded = new List<Card> { card };
        SendToUsedPile(discarded);
        _handManager.RemoveAndReplace(discarded, _deckManager.DrawNextCard);
    }

    void ICardMediator.NotifyEndTurnRequested()
    {
        if (InputLocked || _handManager.IsBusy) return;

        CancelTargeting();
        _comboManager.ClearSelection();

        foreach (ComboBonusDefinition bonus in _comboStarManager.ConcludeTurn())
            _scoreManager.AddBonus(bonus.Name, bonus.Points);

        if (_turnManager.IsLastTurn)
        {
            StartCoroutine(ResolveLastTurn());
            return;
        }

        _turnManager.TryAdvanceTurn();
        _discardManager.ResetDiscards();
        _handManager.ReplaceHand(_deckManager.DrawNextCard, removed =>
        {
            _deckManager.AddToUsedPile(removed);
            _deckManager.ReshuffleUsedIntoDeck();
        });
    }

    void ICardMediator.NotifyItemClicked(int slotIndex)
    {
        if (InputLocked) return;

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

    void ICardMediator.NotifyScoreChanged(int totalScore)
    {
        if (_combatOver) return;

        // Win as soon as the goal is crossed
        if (_encounterManager.IsTargetReached(totalScore))
            FinishEncounter(won: true);
    }

    // ---------- Combat flow ----------

    private IEnumerator ResolveLastTurn()
    {
        _resolvingLastTurn = true;

        // Scores are applied after their announcement, so wait until all of them landed
        yield return new WaitUntil(() => !_scoreManager.IsBusy);

        // If the goal was crossed while waiting, FinishEncounter(true) already ran and this is ignored
        FinishEncounter(_encounterManager.IsTargetReached(_scoreManager.Total));
    }

    private void FinishEncounter(bool won)
    {
        if (_combatOver) return;

        _combatOver = true;
        CancelTargeting();

        _encounterManager.Conclude(won);
        // todo: rewards on win, game over on loss

        StartCoroutine(ReturnToMapAfterDelay());
    }

    private IEnumerator ReturnToMapAfterDelay()
    {
        yield return new WaitForSeconds(_resultDelay);
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
        if (_targetingArrow != null) _targetingArrow.Show();
        // todo: highlight valid targets / the armed slot
    }

    private void CancelTargeting()
    {
        pendingItem = null;
        pendingSlot = -1;
        if (_targetingArrow != null) _targetingArrow.Hide();
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