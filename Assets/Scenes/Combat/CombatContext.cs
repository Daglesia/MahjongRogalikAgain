using System.Collections.Generic;

public class CombatContext
{
    public int NodeId { get; private set; }
    public LocationType LocationType { get; private set; }

    public int Step { get; private set; }
    public int TotalSteps { get; private set; }

    public IReadOnlyList<CardBase> Deck { get; private set; }

    public bool IsBoss => LocationType == LocationType.BossCombat;
    public float Progress => TotalSteps > 0 ? (float)Step / TotalSteps : 0f;

    public static CombatContext FromRun(RunState state, LocationType type)
    {
        MapNode node = state.CurrentNode;

        int totalSteps = 0;
        foreach (MapNode n in state.Map.Nodes)
            if (n.Column > totalSteps) totalSteps = n.Column;

        return new CombatContext
        {
            NodeId = node.Id,
            LocationType = type,
            Step = node.Column,
            TotalSteps = totalSteps,
            Deck = new List<CardBase>(state.Deck)
        };
    }
}