using System.Collections.Generic;
using UnityEngine;

public class CardMediator : MonoBehaviour, ICardMediator
{
    [SerializeField] private ComboManager _comboManager;
    [SerializeField] private DeckManager _deckManager;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardSpawnPosition;
    [SerializeField] private Vector2 comboTarget = new Vector2(-7f, -3.11f);

    private void Awake()
    {
        _comboManager.SetMediator(this);
        _deckManager.SetMediator(this);
    } 

    void ICardMediator.NotifyCardSelected(Card card)
    {
        Debug.Log("yay");
        _comboManager.AddCardToCombo(card);
    }

    void ICardMediator.NotifyComboAssembled(bool isValidCombo, List<Card> selectedCards)
    {
        Debug.Log(isValidCombo + "woop");
        if (isValidCombo)
        {
            var positions = new List<Vector3>();
            foreach (Card card in selectedCards)
            {
                positions.Add(card.GetComponent<Transform>().position);
                card.FlyAndDisappear();
            }
            for(int i = 0; i < selectedCards.Count; i++)
            {
                Instantiate(cardPrefab, cardSpawnPosition.position, Quaternion.identity).GetComponent<Card>().FlyToSpot(positions[i]);
            }
        }
        else
        {
            foreach (Card card in selectedCards)
                card.ResetCard();
        }
    }
}