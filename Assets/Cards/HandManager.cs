using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// Owns everything that physically happens to cards in the hand:
/// spawning, slot management, lifting, flying away, replacing.
/// Other managers only say WHAT should happen (deal, play, discard, select),
/// never HOW the cards move.
public class HandManager : CardColleague
{
    [Header("References")]
    [SerializeField] private HandLayout hand;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardSpawnPosition;

    [Header("Dealing")]
    [SerializeField] private int startingHandSize = 6;
    [SerializeField] private float drawInterval = 0.1f;

    [Header("Animation")]
    [SerializeField] private Vector2 playTarget = new Vector2(-7f, -3.11f);
    [SerializeField] private float liftAmount = 0.3f;
    [SerializeField] private float liftDuration = 0.15f;
    [SerializeField] private float flyDelay = 0.15f;
    [SerializeField] private float flyDuration = 0.5f;

    // ---------- Public API (called by the mediator) ----------

    /// Deals the starting hand one card at a time. drawSource returns null when the deck is empty.
    public void DealStartingHand(Func<CardBase> drawSource)
    {
        StartCoroutine(DealRoutine(drawSource, startingHandSize));
    }

    /// Lifts a card that was just selected.
    public void LiftCard(Card card)
    {
        card.transform.DOKill();
        card.transform.DOMoveY(card.BasePosition.y + liftAmount, liftDuration)
                      .SetEase(Ease.OutQuad)
                      .SetLink(card.gameObject);
    }

    /// Puts cards back to their slots (e.g. invalid combo) and makes them selectable again.
    public void LowerCards(IEnumerable<Card> cards)
    {
        foreach (Card card in cards)
        {
            card.Deselect();
            card.transform.DOKill();
            card.transform.DOMoveY(card.BasePosition.y, liftDuration)
                          .SetEase(Ease.OutQuad)
                          .SetLink(card.gameObject);
        }
    }

    /// Cards fly away and the same number of new cards take their exact slots.
    /// Used for played combos AND for discards (a discard is just a single-card removal).
    public void RemoveAndReplace(IReadOnlyList<Card> cards, Func<CardBase> drawSource)
    {
        // Read slots BEFORE the cards get unparented
        var slots = new List<int>(cards.Count);
        foreach (Card card in cards)
            slots.Add(card.transform.GetSiblingIndex());

        foreach (Card card in cards)
            FlyAndDisappear(card);

        // Ascending order keeps the indices valid as replacements are inserted
        slots.Sort();
        foreach (int slot in slots)
            SpawnIntoHand(drawSource(), slot);
    }

    // ---------- Internals ----------

    private IEnumerator DealRoutine(Func<CardBase> drawSource, int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnIntoHand(drawSource());
            yield return new WaitForSeconds(drawInterval);
        }
    }

    private Card SpawnIntoHand(CardBase cardBase, int siblingIndex = -1)
    {
        if (cardBase == null) return null; // deck empty

        GameObject go = Instantiate(cardPrefab, cardSpawnPosition.position, Quaternion.identity, hand.transform);
        Card card = go.GetComponent<Card>().Initialize(cardBase);
        if (siblingIndex >= 0) go.transform.SetSiblingIndex(siblingIndex);
        return card;
    }

    private void FlyAndDisappear(Card card)
    {
        card.SetInteractable(false);
        card.transform.SetParent(null, true); // leaves the hand -> layout re-flows

        card.transform.DOKill();
        card.transform.DOMove(playTarget, flyDuration)
                      .SetDelay(flyDelay)
                      .SetEase(Ease.InOutQuad)
                      .SetLink(card.gameObject)
                      .OnComplete(() => Destroy(card.gameObject));
    }
}