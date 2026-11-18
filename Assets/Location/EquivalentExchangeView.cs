using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Offers one weighted-random card. UI is built in the prefab, this only fills it in.
public class EquivalentExchangeView : ViewBase
{
    [Header("Content")]
    [SerializeField] private CardPool pool;

    [Header("UI (prefab references)")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardNameLabel;
    [SerializeField] private Button confirmButton;

    private const int Salt = 0x0E0E;

    private CardBase offeredCard;

    protected override void Awake()
    {
        base.Awake();
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    // ---------- ViewBase ----------

    protected override void OnOpen(RunState state)
    {
        offeredCard = RollCard(state);

        Sprite art = offeredCard != null ? offeredCard.art : null;
        cardImage.sprite = art;
        cardImage.enabled = art != null;
        cardNameLabel.text = offeredCard != null ? offeredCard.name : string.Empty;
    }

    public override void UpdateDeck(RunState state)
    {
        // TODO: apply the exchange here, e.g.
        // if (offeredCard != null) state.Deck.Add(offeredCard);
    }

    // ---------- Logic ----------

    private void OnConfirmClicked()
    {
        if (IsBusy) return;

        UpdateDeck(State);
        Close();
    }

    private CardBase RollCard(RunState state)
    {
        if (pool == null)
        {
            Debug.LogError("EquivalentExchangeView: no CardPool assigned.", this);
            return null;
        }

        // Own rng per node: same run + same node => same card, regardless of how much RunManager.Rng was used.
        var rng = new System.Random(DeriveSeed(state.Seed, state.CurrentNodeId));
        return pool.Pick(rng);
    }

    private static int DeriveSeed(int runSeed, int nodeId)
    {
        // Don't use HashCode.Combine: it is randomized per process, so it isn't reproducible.
        unchecked
        {
            return (runSeed * 73856093) ^ (nodeId * 19349663) ^ (Salt * 83492791);
        }
    }
}