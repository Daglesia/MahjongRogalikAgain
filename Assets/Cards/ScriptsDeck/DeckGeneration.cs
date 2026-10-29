using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDeckGeneration", menuName = "Cards/Deck Generation")]
public class DeckGeneration : ScriptableObject
{
    [Serializable]
    public class CardEntry
    {
        public CardBase card;
        [Min(1)] public int count = 1;
    }

    [SerializeField] private List<CardEntry> entries = new List<CardEntry>();

    public Deck GenerateInitial()
    {
        var list = new List<ICard>();

        foreach (CardEntry entry in entries)
        {
            if (entry.card == null) continue;

            for (int i = 0; i < entry.count; i++)
                list.Add(entry.card);
        }

        return new Deck(list);
    }
}