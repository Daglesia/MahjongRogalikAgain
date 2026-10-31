using System;
using System.Collections.Generic;

public enum ComboActiveStatus
    {
        Disabled, NotActive, ActiveBySequence, ActiveByTriplet
    }

public class ComboPieces
{
    private readonly Dictionary<CardValue, ComboActiveStatus> comboPiecesStatuses = new();

    public ComboPieces()
    {
        foreach (CardValue value in Enum.GetValues(typeof(CardValue)))
            comboPiecesStatuses[value] = ComboActiveStatus.NotActive;
    }

    public ComboActiveStatus GetActiveStatus(CardValue value) => comboPiecesStatuses[value];

    public void SetActiveStatus(CardValue value, ComboActiveStatus status)
    {
        comboPiecesStatuses[value] = status;
    }
}
