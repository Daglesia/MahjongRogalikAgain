using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(CardArt))]
public class Card : CardColleague
{
    [SerializeField] private CardBase startingCard;
    [SerializeField] private float liftAmount = 0.3f;

    private CardDefinition cardDefinition;
    private Vector3 basePosition;

    public CardDefinition Definition => cardDefinition;

    void Awake()
    {
        if (startingCard != null) Initialize(startingCard);
    }

    public Card Initialize(CardBase cardBase)
    {
        cardDefinition = new CardDefinition();
        cardDefinition.SetCard(cardBase);
        cardDefinition.SetClicked(false);

        if (TryGetComponent<CardArt>(out var art)) art.Refresh();
        return this;
    }

    public CardBase GetCardBase()
    {
        return cardDefinition.GetCard();
    }

    void Start()
    {
        basePosition = transform.position;
        Mediator = FindAnyObjectByType<CardMediator>();
    }

    void OnMouseDown()
    {
        if (cardDefinition.GetIsClicked()) return;

        cardDefinition.SetClicked(true);
        transform.DOKill();
        transform.DOMoveY(basePosition.y + liftAmount, 0.15f)
                 .SetEase(Ease.OutQuad)
                 .SetLink(gameObject);

        Mediator?.NotifyCardSelected(this);
    }

    public void ResetCard()
    {
        cardDefinition.SetClicked(false);
        transform.DOKill();
        transform.DOMoveY(basePosition.y, 0.15f)
                 .SetEase(Ease.OutQuad)
                 .SetLink(gameObject);
    }

    public void FlyAndDisappear(Vector3 placeToFlyTo)
    {
        GetComponent<Collider2D>().enabled = false;

        transform.DOKill();
        transform.DOMove(new Vector3(placeToFlyTo.x, placeToFlyTo.y, placeToFlyTo.z), 0.5f)
                 .SetDelay(0.15f)
                 .SetEase(Ease.InOutQuad)
                 .SetLink(gameObject)
                 .OnComplete(() => Destroy(gameObject));
    }

    public void FlyToSpot(Vector3 position)
    {
        var col = GetComponent<Collider2D>();
        col.enabled = false;
        transform.DOKill();
        transform.DOMove(position, 0.5f)
                .SetDelay(0.15f)
                .SetEase(Ease.InOutQuad)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    basePosition = position;
                    col.enabled = true;
                });
    }
}