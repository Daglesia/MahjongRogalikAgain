using UnityEngine;

public class CardArt : MonoBehaviour
{
    [SerializeField] private CardDefinition card;
    private SpriteRenderer cardTypeArt;

    void Awake()
    {
        cardTypeArt = transform.Find("CardTypeArt").GetComponent<SpriteRenderer>();
    }

    void Start() => Refresh();

    public void Refresh()
    {
        var sprite = card.GetCardSprite();
        if (sprite != null) cardTypeArt.sprite = sprite;
    }
}