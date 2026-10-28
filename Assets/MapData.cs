
public class CardData : Singleton<RunManager>
{
    public RunState State { get; private set; }
    public System.Random Rng { get; private set; }

    public void StartNewRun(int? seed = null)
    {
        int s = seed ?? System.Environment.TickCount;
        State = new RunState { Seed = s };
        Rng = new System.Random(s);
        // generate map, starting deck...
    }

    public void LoadRun(RunState saved) { /* restore state, rebuild Rng */ }
}