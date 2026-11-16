public abstract class TargetedItemBase : ItemBase
{
    public sealed override bool Use(ItemContext context) => false;

    public abstract bool CanTarget(Card card);

    public abstract bool UseOnCard(ItemContext context, Card card);
}