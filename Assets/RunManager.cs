using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RunState
{
    public int Seed;
    public MapData Map;
    public int CurrentNodeId;          // node the player stands on (0 = Start)
    public List<CardBase> Deck = new(); // the run's card collection, created at run start
    //public List<ModifierData> Modifiers = new();
    public int CurrentFloor;

    public MapNode CurrentNode => Map != null ? Map.Nodes[CurrentNodeId] : null;
}

public class RunManager : Singleton<RunManager>
{
    [SerializeField] private DeckGeneration startingDeck;

    public RunState State { get; private set; }
    public System.Random Rng { get; private set; }

    public void StartNewRun(int? seed = null)
    {
        int s = seed ?? System.Environment.TickCount;
        State = new RunState { Seed = s, CurrentNodeId = 0 };
        Rng = new System.Random(s);

        if (startingDeck == null)
        {
            Debug.LogError("RunManager: no starting deck assigned.", this);
            return;
        }

        State.Deck = startingDeck.GenerateInitialCards();
        Shuffle(State.Deck);
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public void LoadRun(RunState saved)
    {
        State = saved;
        Rng = new System.Random(saved.Seed);
    }
}
