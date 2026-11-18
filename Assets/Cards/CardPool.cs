using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCardPool", menuName = "Cards/Weighted Card Pool")]
public class CardPool : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public CardBase card;
        [Min(0)] public int weight = 1;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    /// Weighted pick. Deterministic for a given rng state. Returns null if the pool has no usable entry.
    public CardBase Pick(System.Random rng)
    {
        int total = 0;
        foreach (Entry e in entries)
            if (e.card != null) total += Mathf.Max(0, e.weight);

        if (total <= 0) return null;

        int roll = rng.Next(total);
        foreach (Entry e in entries)
        {
            if (e.card == null) continue;
            int w = Mathf.Max(0, e.weight);
            if (roll < w) return e.card;
            roll -= w;
        }
        return null;
    }
}
