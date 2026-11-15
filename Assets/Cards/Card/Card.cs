using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(CardArt))]
public class Card : CardColleague
{
    [SerializeField] private CardBase startingCard;

    private CardDefinition cardDefinition;
    private Vector3 basePosition;
    private bool baseSet;

    public CardDefinition Definition => cardDefinition;
    public Vector3 BasePosition => basePosition;

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

    public CardBase GetCardBase() => cardDefinition.GetCard();

    // Called by HandLayout when it assigns this card a slot
    public void SetBasePosition(Vector3 worldPos)
    {
        basePosition = worldPos;
        baseSet = true;
    }

    public void Deselect() => cardDefinition.SetClicked(false);

    public void SetInteractable(bool value) => GetComponent<Collider2D>().enabled = value;

    void Start()
    {
        if (!baseSet) basePosition = transform.position;
        Mediator = FindAnyObjectByType<CardMediator>();
    }

    void OnMouseDown()
    {
        if (cardDefinition.GetIsClicked()) return;

        cardDefinition.SetClicked(true);
        Mediator?.NotifyCardSelected(this);
    }

    void OnMouseOver()
    {
        if (Mouse.current == null || !Mouse.current.rightButton.wasPressedThisFrame) return;
        if (cardDefinition.GetIsClicked()) return;   // selected cards can't be discarded

        Mediator?.NotifyCardDiscardRequested(this);
    }
}