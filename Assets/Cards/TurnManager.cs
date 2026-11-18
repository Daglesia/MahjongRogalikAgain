using System;
using UnityEngine;

public class TurnManager : CardColleague
{
    [SerializeField, Min(1)] private int maxTurns = 3;

    private int currentTurn = 1;

    public int CurrentTurn => currentTurn;
    public int MaxTurns => maxTurns;
    public bool IsLastTurn => currentTurn >= maxTurns;

    public event Action<int, int> TurnChanged;   // current, max

    /// Returns false when there are no turns left.
    public bool TryAdvanceTurn()
    {
        if (IsLastTurn) return false;

        currentTurn++;
        TurnChanged?.Invoke(currentTurn, maxTurns);
        return true;
    }
}