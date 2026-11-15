using UnityEngine;

public abstract class ItemBase : ScriptableObject
{
    [SerializeField] public Sprite icon;
    [SerializeField] private string itemName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private bool consumable = true;

    public string ItemName => itemName;
    public string Description => description;
    public bool Consumable => consumable;

    /// Returns true if the item actually did something (so it only gets consumed on success).
    public abstract bool Use(ItemContext context);
}