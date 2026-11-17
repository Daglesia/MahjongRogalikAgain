using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    public const string MapSceneName = "MapScene";
    public const string CombatSceneName = "CombatScene";

    /// Set while a combat is running, null otherwise.
    public CombatContext CurrentCombat { get; private set; }

    private bool loading;

    public void StartCombat(CombatContext context)
    {
        if (loading) return;
        CurrentCombat = context;
        StartCoroutine(Load(CombatSceneName));
    }

    /// Call this when combat ends (win/lose handling can be added before it).
    public void ReturnToMap()
    {
        if (loading) return;
        CurrentCombat = null;
        StartCoroutine(Load(MapSceneName));
    }

    private IEnumerator Load(string sceneName)
    {
        loading = true;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!op.isDone) yield return null;

        loading = false;
    }
}