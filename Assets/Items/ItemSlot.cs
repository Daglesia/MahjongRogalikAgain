using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ItemSlot : CardColleague
{
    private int index;
    private SpriteRenderer sr;
    private Collider2D col;

    public void Init(int slotIndex)
    {
        index = slotIndex;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Start() => Mediator = FindAnyObjectByType<CardMediator>();

    public void Show(ItemBase item)
    {
        if (sr == null) Init(index);
        sr.sprite = item != null ? item.icon : null;
        sr.enabled = item != null;
        col.enabled = item != null;
    }

    void OnMouseDown() => Mediator?.NotifyItemClicked(index);
}