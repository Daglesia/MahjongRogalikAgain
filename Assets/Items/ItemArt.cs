using UnityEngine;

[RequireComponent(typeof(ItemSlot))]
public class ItemArt : MonoBehaviour
{
    private ItemSlot slot;
    private SpriteRenderer itemWrapper;
    private SpriteRenderer itemArt;

    private void EnsureReferences()
    {
        if (slot == null) slot = GetComponent<ItemSlot>();
        if (itemWrapper == null)
            itemWrapper = transform.Find("ItemWrapper").GetComponent<SpriteRenderer>();
        if (itemArt == null)
            itemArt = transform.Find("ItemArt").GetComponent<SpriteRenderer>();
    }

    public void Refresh()
    {
        EnsureReferences();
        var item = slot.CurrentItem;
        var sprite = item != null ? item.icon : null;

        if (itemWrapper != null) itemWrapper.enabled = item != null;
        itemArt.enabled = item != null;
        if (sprite != null) itemArt.sprite = sprite;
    }
}
