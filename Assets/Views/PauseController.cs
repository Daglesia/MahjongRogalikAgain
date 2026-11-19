using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// Opens the PauseView on Escape / lost focus (MapScene + CombatScene only)
/// and freezes the game while it is open.
public class PauseController : Singleton<PauseController>
{
    private PauseView view;
    private float previousTimeScale = 1f;
    private bool paused;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap() => _ = Instance;   // no scene setup needed

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;   // duplicate, about to be destroyed

        var go = new GameObject("PauseView");
        go.transform.SetParent(transform, false);   // persists together with this object
        view = go.AddComponent<PauseView>();

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (paused) view.Close();
        else Pause();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) Pause();
    }

    public void Pause()
    {
        if (paused || view == null || !InPausableScene()) return;

        paused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;          // freezes DOTween, WaitForSeconds, drawing animations...
        AudioListener.pause = true;

        view.Open(null, OnClosed);    // the state isn't used by this view
    }

    private void OnClosed()
    {
        paused = false;
        Time.timeScale = previousTimeScale;
        AudioListener.pause = false;
    }

    private void OnActiveSceneChanged(Scene from, Scene to)
    {
        // Never carry a pause into another scene (e.g. combat -> map, or the menu)
        if (paused) view.Close();
    }

    private static bool InPausableScene()
    {
        string scene = SceneManager.GetActiveScene().name;
        return scene == SceneLoader.MapSceneName || scene == SceneLoader.CombatSceneName;
    }
}