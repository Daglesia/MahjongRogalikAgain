using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Shop screen: shows a few deterministic card offers. Taking one adds it to the run deck.
public class ShopView : ViewBase
{
    [Serializable]
    private class OfferSlot
    {
        public Image cardImage;
        public TextMeshProUGUI nameLabel;
        public Button buyButton;
    }

    [Header("Content")]
    [SerializeField] private CardPool pool;
    [SerializeField, Range(1, 3)] private int offerCount = 3;

    [Header("UI (prefab references)")]
    [SerializeField] private OfferSlot[] slots;
    [SerializeField] private Button leaveButton;

    private const int Salt = 0x5A0F;

    private readonly List<CardBase> offers = new List<CardBase>();
    private CardBase selectedCard;

    protected override void Awake()
    {
        base.Awake();

        if (slots == null || slots.Length == 0)
            Debug.LogError("ShopView: no offer slots assigned on the prefab.", this);
        if (leaveButton == null)
            Debug.LogError("ShopView: 'Leave' button is not assigned on the prefab.", this);

        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                int index = i;
                if (slots[i]?.buyButton != null)
                    slots[i].buyButton.onClick.AddListener(() => OnOfferClicked(index));
            }
        }

        if (leaveButton != null)
            leaveButton.onClick.AddListener(OnLeaveClicked);
    }

    // ---------- ViewBase ----------

    protected override void OnOpen(RunState state)
    {
        selectedCard = null;
        offers.Clear();

        if (state == null) return;

        if (pool == null)
            Debug.LogError("ShopView: no CardPool assigned.", this);

        // Own rng per node: same run + same node => same offers.
        var rng = new System.Random(DeriveSeed(state.Seed, state.CurrentNodeId));
        int count = Mathf.Min(offerCount, slots?.Length ?? 0);
        for (int i = 0; i < count; i++)
            offers.Add(pool != null ? pool.Pick(rng) : null);

        for (int i = 0; i < (slots?.Length ?? 0); i++)
        {
            OfferSlot slot = slots[i];
            if (slot == null) continue;

            CardBase card = i < count ? offers[i] : null;
            Sprite art = card != null ? card.art : null;

            if (slot.cardImage != null)
            {
                slot.cardImage.sprite = art;
                slot.cardImage.enabled = art != null;
            }

            if (slot.nameLabel != null)
                slot.nameLabel.text = card != null ? card.name : string.Empty;

            if (slot.buyButton != null)
                slot.buyButton.interactable = card != null;
        }
    }

    public override void UpdateDeck(RunState state)
    {
        if (selectedCard == null || state?.Deck == null) return;

        state.Deck.Add(selectedCard);
        selectedCard = null;   // avoid adding the same card twice if called again
    }

    // ---------- Logic ----------

    private void OnOfferClicked(int index)
    {
        if (IsBusy || index < 0 || index >= offers.Count || offers[index] == null) return;

        selectedCard = offers[index];
        UpdateDeck(State);
        Close();
    }

    private void OnLeaveClicked()
    {
        if (IsBusy) return;
        Close();
    }

    private static int DeriveSeed(int runSeed, int nodeId)
    {
        unchecked
        {
            return (runSeed * 73856093) ^ (nodeId * 19349663) ^ (Salt * 83492791);
        }
    }
}
