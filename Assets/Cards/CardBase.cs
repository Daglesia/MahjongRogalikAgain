using UnityEngine;

public abstract class CardBase : ScriptableObject, ICard
{
    [SerializeField] public Sprite art;
    public abstract CardValue[] GetCardValues();
    public abstract string GetDescription();
}