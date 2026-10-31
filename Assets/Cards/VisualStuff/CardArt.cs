using UnityEngine;

[RequireComponent(typeof(Card))]
public class CardArt : MonoBehaviour
{
    private Card card;
    private SpriteRenderer cardTypeArt;

    private void EnsureReferences()
    {
        if (card == null) card = GetComponent<Card>();
        if (cardTypeArt == null)
            cardTypeArt = transform.Find("CardTypeArt").GetComponent<SpriteRenderer>();
    }

    public void Refresh()
    {
        EnsureReferences();
        var sprite = card.Definition?.GetCardSprite();
        if (sprite != null) cardTypeArt.sprite = sprite;
    }
}