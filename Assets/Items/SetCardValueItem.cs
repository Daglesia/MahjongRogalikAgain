using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSetCardValueItem", menuName = "Items/Set Card Value")]
public class SetCardValueItem : TargetedItemBase
{
    [Tooltip("The card asset the target gets turned into (e.g. Bamboo3.asset)")]
    [SerializeField] private CardBase resultCard;

    [Tooltip("Values this item may be used on. Empty = any card.")]
    [SerializeField] private CardValue[] allowedValues = Array.Empty<CardValue>();

    public override bool CanTarget(Card card)
    {
        if (resultCard == null || card == null) return false;

        CardBase current = card.GetCardBase();
        if (current == null || current == resultCard) return false;   // no point wasting it

        if (allowedValues.Length == 0) return true;

        foreach (CardValue v in current.GetCardValues())
            if (Array.IndexOf(allowedValues, v) >= 0) return true;

        return false;
    }

    public override bool UseOnCard(ItemContext context, Card card)
    {
        if (!CanTarget(card)) return false;

        card.ReplaceCardBase(resultCard);
        return true;
    }
}