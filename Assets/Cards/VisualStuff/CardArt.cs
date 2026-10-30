using UnityEngine;

[RequireComponent(typeof(Card))]
public class CardArt : MonoBehaviour
{
    private Card card;
    private SpriteRenderer cardTypeArt;

    void Awake()
    {
        card = GetComponent<Card>();
        cardTypeArt = transform.Find("CardTypeArt").GetComponent<SpriteRenderer>();
    }

    // void Start() => Refresh();

    public void Refresh()
    {
        var sprite = card.Definition?.GetCardSprite();
        if (sprite != null) cardTypeArt.sprite = sprite;
    }
}