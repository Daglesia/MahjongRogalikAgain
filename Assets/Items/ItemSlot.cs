using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(ItemArt))]
public class ItemSlot : CardColleague
{
    private int index;
    private Collider2D col;
    private ItemArt art;

    public ItemBase CurrentItem { get; private set; }

    public void Init(int slotIndex)
    {
        index = slotIndex;
        col = GetComponent<Collider2D>();
        art = GetComponent<ItemArt>();
    }

    void Start() => Mediator = FindAnyObjectByType<CardMediator>();

    public void Show(ItemBase item)
    {
        if (art == null) Init(index);
        CurrentItem = item;
        art.Refresh();
        col.enabled = item != null;
    }

    void OnMouseDown() => Mediator?.NotifyItemClicked(index);
}
