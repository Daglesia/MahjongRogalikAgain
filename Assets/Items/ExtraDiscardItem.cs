using UnityEngine;

[CreateAssetMenu(fileName = "NewExtraDiscardItem", menuName = "Items/Extra Discard")]
public class ExtraDiscardItem : ItemBase
{
    [SerializeField, Min(1)] private int amount = 1;

    public override bool Use(ItemContext context)
    {
        context.Discards.AddDiscards(amount);
        return true;
    }
}