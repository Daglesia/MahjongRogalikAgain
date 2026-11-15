using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : CardColleague
{
    public const int MaxItems = 2;

    [SerializeField] private ItemSlot[] slots = new ItemSlot[MaxItems];
    [SerializeField] private List<ItemBase> startingItems = new List<ItemBase>();

    private readonly ItemBase[] items = new ItemBase[MaxItems];

    public bool IsFull
    {
        get
        {
            foreach (var item in items) if (item == null) return false;
            return true;
        }
    }

    public event Action<int, ItemBase> SlotChanged;   // slotIndex, item (null = emptied)

    void Awake()
    {
        for (int i = 0; i < slots.Length && i < MaxItems; i++)
            slots[i].Init(i);
    }

    void Start()
    {
        foreach (var item in startingItems)
            TryAddItem(item);
        RefreshAllSlots();
    }

    public ItemBase GetItem(int index) =>
        index >= 0 && index < MaxItems ? items[index] : null;

    public bool TryAddItem(ItemBase item)
    {
        if (item == null) return false;

        for (int i = 0; i < MaxItems; i++)
        {
            if (items[i] != null) continue;
            items[i] = item;
            SetSlot(i, item);
            return true;
        }
        return false; // inventory full
    }

    public void RemoveItem(int index)
    {
        if (GetItem(index) == null) return;
        items[index] = null;
        SetSlot(index, null);
    }

    private void SetSlot(int index, ItemBase item)
    {
        if (index < slots.Length && slots[index] != null) slots[index].Show(item);
        SlotChanged?.Invoke(index, item);
    }

    private void RefreshAllSlots()
    {
        for (int i = 0; i < MaxItems; i++) SetSlot(i, items[i]);
    }
}