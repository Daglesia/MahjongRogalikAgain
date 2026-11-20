using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(CardArt))]
public class Card : CardColleague
{
    [SerializeField] private CardBase startingCard;

    [Header("Hover")]
    [Tooltip("1 = no change. 1.06 = 6% bigger.")]
    [SerializeField, Min(1f)] private float hoverScale = 1.06f;
    [SerializeField, Min(0f)] private float hoverDuration = 0.12f;

    private CardDefinition cardDefinition;
    private Vector3 basePosition;
    private bool baseSet;
    private Vector3 baseScale;
    private Tween hoverTween;

    public CardDefinition Definition => cardDefinition;
    public Vector3 BasePosition => basePosition;
    /// Scale without the hover effect (HandLayout uses it so hovering never changes the layout).
    public Vector3 BaseScale => baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
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

    public CardBase GetCardBase() => cardDefinition.GetCard();

    // Called by HandLayout when it assigns this card a slot
    public void SetBasePosition(Vector3 worldPos)
    {
        basePosition = worldPos;
        baseSet = true;
    }

    public void Deselect() => cardDefinition.SetClicked(false);

    public void SetInteractable(bool value)
    {
        GetComponent<Collider2D>().enabled = value;

        if (!value)
        {
            // A disabled collider never sends OnMouseExit, so reset the hover by hand
            hoverTween?.Kill();
            transform.localScale = baseScale;
        }
    }

    void Start()
    {
        if (!baseSet) basePosition = transform.position;
        Mediator = FindAnyObjectByType<CardMediator>();
    }

    void OnMouseEnter()
    {
        if (DeckView.AnyOpen) return;
        SetHovered(true);
    }

    void OnMouseExit() => SetHovered(false);

    void OnMouseDown()
    {
        if (cardDefinition.GetIsClicked())
        {
            // clicking a selected card un-selects it
            Mediator?.NotifyCardDeselected(this);
            return;
        }

        cardDefinition.SetClicked(true);
        Mediator?.NotifyCardSelected(this);
    }

    void OnMouseOver()
    {
        if (Mouse.current == null || !Mouse.current.rightButton.wasPressedThisFrame) return;
        if (cardDefinition.GetIsClicked()) return;   // selected cards can't be discarded

        Mediator?.NotifyCardDiscardRequested(this);
    }

    public void ReplaceCardBase(CardBase newBase)
    {
        cardDefinition.SetCard(newBase);
        if (TryGetComponent<CardArt>(out var art)) art.Refresh();
    }

    private void SetHovered(bool hovered)
    {
        hoverTween?.Kill();

        Vector3 target = hovered ? baseScale * hoverScale : baseScale;
        hoverTween = transform.DOScale(target, hoverDuration)
                              .SetEase(Ease.OutQuad)
                              .SetTarget(this)               // NOT the transform: HandManager calls transform.DOKill() for the lift
                              .SetLink(gameObject);
    }
}