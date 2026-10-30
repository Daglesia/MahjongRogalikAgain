using UnityEngine;

public class CardDefinition : MonoBehaviour
{
    [SerializeField] private CardBase card;
    private bool isClicked = false;

    public void SetCard(CardBase newCard) => card = newCard;

    public Sprite GetCardSprite() => card != null ? card.art : null;
    public CardBase GetCard() => card;
    public bool GetIsClicked() => isClicked;
    public void SetClicked(bool flag) => isClicked = flag;
}