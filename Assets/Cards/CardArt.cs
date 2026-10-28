using UnityEngine;

public class CardArt : MonoBehaviour
{
    [SerializeField] private CardDefinition card;
    SpriteRenderer cardTypeArt;
    void Start()
    {
        cardTypeArt = transform.Find("CardTypeArt").GetComponent<SpriteRenderer>();
        cardTypeArt.sprite = card.GetCardSprite();
    }
}
