using System.Collections.Generic;
using UnityEngine;

public class DeckGeneration: MonoBehaviour
{
    [SerializeField] private CardBase cards;
    public Deck GenerateInitial()
    {
        return new Deck(cards);
    }
}