using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class Card : CardColleague
{
    private CardDefinition cardDefinition;
    [SerializeField] private float liftAmount = 0.3f;

    public void Initialize(CardBase cardBase)
    {
        cardDefinition = new CardDefinition();
        cardDefinition.SetCard(cardBase);
        cardDefinition.SetClicked(false);
    }

    public CardBase GetCardBase()
    {
        return cardDefinition.GetCard();
    }

    private Vector3 basePosition;

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
        GetComponent<Collider2D>().enabled = false;
        transform.DOKill();
        transform.DOMove(position, 0.5f)
                 .SetDelay(0.15f)
                 .SetEase(Ease.InOutQuad)
                 .SetLink(gameObject);
        GetComponent<Collider2D>().enabled = true;
    }
}