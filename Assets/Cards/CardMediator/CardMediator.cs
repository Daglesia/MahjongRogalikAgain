using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CardMediator : MonoBehaviour, ICardMediator
{
    [SerializeField] private ComboManager _comboManager;
    [SerializeField] private DeckManager _deckManager;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardSpawnPosition;
    [SerializeField] private Vector2 comboTarget = new Vector2(-7f, -3.11f);

    [SerializeField] private HandLayout hand; 

    [SerializeField] private int startingHandSize = 6;
    [SerializeField] private float drawInterval = 0.1f;

    private void Start() => StartCoroutine(DrawStartingHand());

    private Card SpawnCardIntoHand(CardBase cardBase, int siblingIndex = -1)
    {
        if (cardBase == null) return null;

        var go = Instantiate(cardPrefab, cardSpawnPosition.position, Quaternion.identity, hand.transform);
        var card = go.GetComponent<Card>().Initialize(cardBase);
        if (siblingIndex >= 0) go.transform.SetSiblingIndex(siblingIndex);
        return card;
    }

    private IEnumerator DrawStartingHand()
    {
        for (int i = 0; i < startingHandSize; i++)
        {
            SpawnCardIntoHand(_deckManager.DrawNextCard());
            yield return new WaitForSeconds(drawInterval);
        }
    }
    

    private void Awake()
    {
        _comboManager.SetMediator(this);
        _deckManager.SetMediator(this);


    }

    void ICardMediator.NotifyCardSelected(Card card)
    {
        _comboManager.AddCardToCombo(card);
    }

void ICardMediator.NotifyComboAssembled(bool isValidCombo, List<Card> selectedCards)
{
    if (isValidCombo)
    {
        var slots = new List<int>();
        foreach (Card card in selectedCards)
            slots.Add(card.transform.GetSiblingIndex());   // read BEFORE they unparent

        foreach (Card card in selectedCards)
            card.FlyAndDisappear(comboTarget);

        slots.Sort();
        foreach (int slot in slots)
            SpawnCardIntoHand(_deckManager.DrawNextCard(), slot);
    }
    else
    {
        foreach (Card card in selectedCards)
            card.ResetCard();
    }
}
}