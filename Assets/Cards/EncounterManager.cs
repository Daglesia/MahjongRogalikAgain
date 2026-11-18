using System;
using UnityEngine;

/// Owns the goal of the current combat: how many points are needed to win.
public class EncounterManager : CardColleague
{
    [Header("Target score = (base + step * perStep) * (boss ? bossMultiplier : 1)")]
    [SerializeField, Min(0)] private int baseTarget = 40;
    [SerializeField, Min(0)] private int perStep = 10;
    [SerializeField, Min(1f)] private float bossMultiplier = 1.5f;

    public int TargetScore { get; private set; }

    public event Action<bool> EncounterEnded;   // true = won

    void Awake()
    {
        CombatContext ctx = SceneLoader.Instance.CurrentCombat;

        // ctx is null when CombatScene is played directly in the editor
        TargetScore = ctx != null ? CalculateTarget(ctx) : baseTarget;
    }

    public bool IsTargetReached(int score) => score >= TargetScore;

    public void Conclude(bool won) => EncounterEnded?.Invoke(won);

    private int CalculateTarget(CombatContext ctx)
    {
        float target = baseTarget + ctx.Step * perStep;
        if (ctx.IsBoss) target *= bossMultiplier;
        return Mathf.RoundToInt(target);
    }
}