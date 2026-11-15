using System;
using UnityEngine;

public class DiscardManager : CardColleague
{
    [SerializeField] private int startingDiscards = 3;

    private int discardsLeft;

    public int Count => discardsLeft;
    public bool CanDiscard => discardsLeft > 0;
    public event Action<int> DiscardsChanged;

    void Awake()
    {
        discardsLeft = startingDiscards;
    }

    public bool TryUseDiscard()
    {
        if (!CanDiscard) return false;

        discardsLeft--;
        DiscardsChanged?.Invoke(discardsLeft);
        return true;
    }
}