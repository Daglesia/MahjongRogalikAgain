using TMPro;
using UnityEngine;

public class CardsLeftText : MonoBehaviour
{
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private TextMeshProUGUI label;

    void Start()
    {
        // deckManager.GetCount();
    }

    void OnEnable()
    {
        deckManager.CardDrawn += OnCardDrawn;
        deckManager.DeckReshuffled += OnDeckReshuffled;
    }

    void OnDisable()
    {
        deckManager.CardDrawn -= OnCardDrawn;
        deckManager.DeckReshuffled -= OnDeckReshuffled;
    }

    private void OnDeckReshuffled(int cardsLeft) => label.text = cardsLeft.ToString();

    private void OnCardDrawn(CardBase card, int cardsLeft)
    {
        label.text = cardsLeft.ToString();
    }
}