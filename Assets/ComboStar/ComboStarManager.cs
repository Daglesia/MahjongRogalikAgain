using System;
using System.Collections.Generic;

public class ComboStarManager : CardColleague
{
    public readonly struct MadeCombo
    {
        public readonly ComboType Type;
        public readonly CardValue[] Values;
        public MadeCombo(ComboType type, CardValue[] values) { Type = type; Values = values; }
    }

    private readonly ComboPieces pieces = new ComboPieces();
    private readonly List<MadeCombo> combosThisTurn = new List<MadeCombo>();

    public IReadOnlyList<MadeCombo> CombosThisTurn => combosThisTurn;
    public ComboActiveStatus GetStatus(CardValue value) => pieces.GetActiveStatus(value);

    // For the star UI
    public event Action<CardValue, ComboActiveStatus> PieceChanged;
    public event Action TurnReset;

    /// Called by the mediator right after a valid combo was assembled.
    public void RegisterCombo(ComboType type, IReadOnlyList<CardValue> values)
    {
        if (type == ComboType.None || values == null) return;

        ComboActiveStatus status = type == ComboType.Triplet
            ? ComboActiveStatus.ActiveByTriplet
            : ComboActiveStatus.ActiveBySequence;

        var copy = new CardValue[values.Count];
        for (int i = 0; i < copy.Length; i++) copy[i] = values[i];
        combosThisTurn.Add(new MadeCombo(type, copy));

        foreach (CardValue value in copy)
            Apply(value, status);
    }

    /// End of turn: returns every bonus whose requirements are met, then resets the star.
    public List<ComboBonusDefinition> ConcludeTurn()
    {
        var earned = new List<ComboBonusDefinition>();
        foreach (ComboBonusDefinition bonus in ComboBonusLibrary.All)
            if (bonus.IsSatisfiedBy(pieces)) earned.Add(bonus);

        combosThisTurn.Clear();
        pieces.Reset();
        TurnReset?.Invoke();
        return earned;
    }

    private void Apply(CardValue value, ComboActiveStatus status)
    {
        ComboActiveStatus current = pieces.GetActiveStatus(value);

        if (current == ComboActiveStatus.Disabled) return;
        if (current == ComboActiveStatus.ActiveByTriplet) return;   // a triplet is never downgraded to a sequence
        if (current == status) return;

        pieces.SetActiveStatus(value, status);
        PieceChanged?.Invoke(value, status);
    }
}