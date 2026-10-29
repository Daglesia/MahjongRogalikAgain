using UnityEngine;

public class CardDefinition : MonoBehaviour
{
    [SerializeField] private CardBase card;
    private bool isClicked = false;

    public Sprite GetCardSprite() => card.art;
    public CardBase GetCard() => card;
    public bool GetIsClicked() => isClicked;
    public void SetClicked(bool flag) => isClicked = flag; 
}
