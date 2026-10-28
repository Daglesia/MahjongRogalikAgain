using UnityEngine;

[CreateAssetMenu(fileName = "NewBasicCard", menuName = "Cards/Basic Card")]
public class BasicCard : CardBase
{
    [SerializeField] private CardValue value;
    [SerializeField, TextArea] private string description;

    public CardValue Value => value;

    public override CardValue[] GetCardValues() => new[] { value };
    public Sprite GetSprite() => art;
    public override string GetDescription() => description;
}