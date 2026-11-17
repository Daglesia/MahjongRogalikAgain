using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Serializable]
    public struct WeightedType
    {
        public LocationType type;
        [Min(0)] public int weight;
        public WeightedType(LocationType type, int weight) { this.type = type; this.weight = weight; }
    }

    [Header("Shape")]
    [Tooltip("Number of 'other' steps between fixed Combats. The last segment ends in the Boss.\n" +
             "{3,3,2} = Start, Combat, 3 others, Combat, 3 others, Combat, 2 others, Boss")]
    [SerializeField] private int[] segmentLengths = { 3, 3, 2 };
    [Tooltip("Each segment has exactly ONE fork, with a random start and a length in this range (clamped to the segment length).")]
    [SerializeField, Min(1)] private int minForkLength = 1;
    [SerializeField, Min(1)] private int maxForkLength = 3;

    [Header("Layout")]
    [SerializeField] private float columnSpacing = 1.5f;
    [SerializeField] private float laneOffset = 1.2f;
    [SerializeField] private float jitter = 0.15f;

    [Header("Weights for 'other' steps (Start / Combat / BossCombat are ignored)")]
    [SerializeField]
    private List<WeightedType> weights = new List<WeightedType>
    {
        new WeightedType(LocationType.Shop, 2),
        new WeightedType(LocationType.EquivalentExchange, 2),
        new WeightedType(LocationType.Stone, 2),
        new WeightedType(LocationType.CardFreebie, 2),
        new WeightedType(LocationType.BlackHole, 1),
    };

    private int totalColumns;

    /// Pure: uses only System.Random(seed), creates no GameObjects.
    public MapData GenerateMap(int seed)
    {
        var rng = new System.Random(seed);
        var map = new MapData { Seed = seed };

        // Start + Combat + segments + (segments-1) combats + Boss
        int sum = 0;
        foreach (int s in segmentLengths) sum += Mathf.Max(0, s);
        totalColumns = 2 + sum + Mathf.Max(0, segmentLengths.Length - 1) + 1;

        int c = 0;

        // Column 0: Start, column 1: Combat (fixed)
        var start = AddNode(map, rng, c++, 0, LocationType.Start, false);
        var combat = AddNode(map, rng, c++, 0, LocationType.Combat, false);
        Link(new List<MapNode> { start }, new List<MapNode> { combat });
        var prevColumn = new List<MapNode> { combat };

        for (int seg = 0; seg < segmentLengths.Length; seg++)
        {
            int segLen = Mathf.Max(0, segmentLengths[seg]);

            // Exactly one fork per segment: random length, random start.
            // Always draw both numbers so a seed stays deterministic.
            int lo = Mathf.Max(1, minForkLength);
            int hi = Mathf.Max(lo, maxForkLength);
            int lenRoll = rng.Next(lo, hi + 1);
            int forkLen = Mathf.Min(lenRoll, segLen);
            int startRoll = rng.Next(0, Mathf.Max(1, segLen - forkLen + 1));
            int forkStart = startRoll;

            for (int i = 0; i < segLen; i++)
            {
                bool inFork = forkLen > 0 && i >= forkStart && i < forkStart + forkLen;

                if (!inFork)
                {
                    var node = AddNode(map, rng, c++, 0, PickType(rng), true);
                    Link(prevColumn, new List<MapNode> { node });   // single->single, or 2 lanes merge
                    prevColumn = new List<MapNode> { node };
                }
                else
                {
                    var upper = AddNode(map, rng, c, +1, PickType(rng), true);

                    LocationType lowerType = PickType(rng);
                    for (int t = 0; t < 3 && lowerType == upper.Type; t++)
                        lowerType = PickType(rng);
                    var lower = AddNode(map, rng, c, -1, lowerType, true);
                    c++;

                    if (prevColumn.Count == 1)
                    {
                        // diverge: single -> both lanes
                        Link(prevColumn, new List<MapNode> { upper, lower });
                    }
                    else
                    {
                        // straight across, no crossing
                        prevColumn[0].NextIds.Add(upper.Id);
                        prevColumn[1].NextIds.Add(lower.Id);
                    }

                    prevColumn = new List<MapNode> { upper, lower };
                }
            }

            // After every segment: a single node that all lanes converge on
            bool isLast = seg == segmentLengths.Length - 1;
            var gate = AddNode(map, rng, c++, 0,
                isLast ? LocationType.BossCombat : LocationType.Combat, false);
            Link(prevColumn, new List<MapNode> { gate });
            prevColumn = new List<MapNode> { gate };
        }

        return map;
    }

    // ---------------- helpers ----------------

    private MapNode AddNode(MapData map, System.Random rng, int column, int lane, LocationType type, bool useJitter)
    {
        // Always consume two numbers so layout never shifts the random sequence
        float jx = (float)(rng.NextDouble() * 2.0 - 1.0) * jitter;
        float jy = (float)(rng.NextDouble() * 2.0 - 1.0) * jitter;
        if (!useJitter) { jx = 0f; jy = 0f; }

        float x = (column - (totalColumns - 1) / 2f) * columnSpacing + jx;
        float y = lane * laneOffset + jy;

        var node = new MapNode
        {
            Id = map.Nodes.Count,
            Column = column,
            Lane = lane,
            Type = type,
            Position = new Vector2(x, y)
        };
        map.Nodes.Add(node);
        return node;
    }

    /// Every node in "from" connects to every node in "to".
    private static void Link(List<MapNode> from, List<MapNode> to)
    {
        foreach (var a in from)
            foreach (var b in to)
                a.NextIds.Add(b.Id);
    }

    private LocationType PickType(System.Random rng)
    {
        int total = 0;
        foreach (var w in weights)
            if (IsRollable(w.type)) total += w.weight;

        int roll = rng.Next(Mathf.Max(1, total));   // always consumes one number
        if (total <= 0) return LocationType.Shop;

        foreach (var w in weights)
        {
            if (!IsRollable(w.type)) continue;
            if (roll < w.weight) return w.type;
            roll -= w.weight;
        }
        return LocationType.Shop;
    }

    private static bool IsRollable(LocationType t) =>
        t != LocationType.Start && t != LocationType.Combat && t != LocationType.BossCombat;
}