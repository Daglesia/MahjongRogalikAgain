using System.Collections.Generic;

public readonly struct ComboRequirement
{
    public readonly CardValue Value;
    public readonly ComboActiveStatus Status;

    public ComboRequirement(CardValue value, ComboActiveStatus status)
    {
        Value = value;
        Status = status;
    }
}

public class ComboBonusDefinition
{
    public string Name { get; }
    public int Points { get; }
    public IReadOnlyList<ComboRequirement> Requirements { get; }

    public ComboBonusDefinition(string name, int points, params ComboRequirement[] requirements)
    {
        Name = name;
        Points = points;
        Requirements = requirements;
    }

    public bool IsSatisfiedBy(ComboPieces pieces)
    {
        foreach (ComboRequirement r in Requirements)
            if (pieces.GetActiveStatus(r.Value) != r.Status) return false;
        return true;
    }
}

/// All bonuses that can be earned at the end of a turn. Add new ones here.
public static class ComboBonusLibrary
{
    public static readonly IReadOnlyList<ComboBonusDefinition> All = new[]
    {
        new ComboBonusDefinition("Full Straight", 50,
            Require(ComboActiveStatus.ActiveBySequence,
                CardValue.Bamboo1, CardValue.Bamboo2, CardValue.Bamboo3,
                CardValue.Bamboo4, CardValue.Bamboo5, CardValue.Bamboo6)),

        new ComboBonusDefinition("21st", 500,
            Require(ComboActiveStatus.ActiveByTriplet,
                CardValue.Earth, CardValue.Wind, CardValue.Water, CardValue.Fire)),
    };

    private static ComboRequirement[] Require(ComboActiveStatus status, params CardValue[] values)
    {
        var result = new ComboRequirement[values.Length];
        for (int i = 0; i < values.Length; i++)
            result[i] = new ComboRequirement(values[i], status);
        return result;
    }
}