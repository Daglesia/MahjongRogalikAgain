using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class Card : CardColleague
{
    [SerializeField] private CardDefinition cardDefinition;
    [SerializeField] private float liftAmount = 0.3f;

    public CardBase GetCardBase()
    {
        return cardDefinition.GetCard();
    }

    private Vector3 basePosition;
    [SerializeField] private Transform placeToFlyTo;

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

    public void FlyAndDisappear()
    {
        GetComponent<Collider2D>().enabled = false;

        transform.DOKill();
        transform.DOMove(new Vector3(placeToFlyTo.position.x, placeToFlyTo.position.y, transform.position.z), 0.5f)
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
                 .SetLink(gameObject)
                 .OnComplete(() => Destroy(gameObject));
        GetComponent<Collider2D>().enabled = true;
    }
}