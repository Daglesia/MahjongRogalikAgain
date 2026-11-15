using UnityEngine;
using UnityEngine.InputSystem;
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
    private bool baseSet;

    public void SetBasePosition(Vector3 worldPos)
    {
        basePosition = worldPos;
        baseSet = true;
    }

    void Start()
    {
        if (!baseSet) basePosition = transform.position;
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

    void OnMouseOver()
    {
        if (Mouse.current == null || !Mouse.current.rightButton.wasPressedThisFrame) return;
        if (cardDefinition.GetIsClicked()) return;   // selected cards can't be discarded

        Mediator?.NotifyCardDiscardRequested(this);
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
        transform.SetParent(null, true);

        transform.DOKill();
        transform.DOMove(placeToFlyTo, 0.5f)
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