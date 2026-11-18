using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : Singleton<SceneLoader>
{
    public const string MapSceneName = "MapScene";
    public const string CombatSceneName = "CombatScene";

    [Header("Transition")]
    [SerializeField, Min(0f)] private float fadeInDuration = 0.15f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.2f;
    [SerializeField, Min(0f)] private float blackHoldDuration = 0.05f;
    [SerializeField, Min(0f)] private float revealDelayDuration = 0.05f;

    /// Set while a combat is running, null otherwise.
    public CombatContext CurrentCombat { get; private set; }

    private bool loading;
    private CanvasGroup fadeGroup;

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

    /// Fades to black, runs onBlack while the screen is fully black, then fades back in and calls onDone.
    /// Returns false (and does nothing) if another transition/scene load is already running.
    public bool Transition(Action onBlack, Action onDone = null)
    {
        if (loading) return false;
        StartCoroutine(TransitionRoutine(onBlack, onDone));
        return true;
    }

    private IEnumerator TransitionRoutine(Action onBlack, Action onDone)
    {
        loading = true;

        EnsureFadeOverlay();
        fadeGroup.blocksRaycasts = true;
        yield return Fade(0f, 1f, fadeInDuration);

        if (blackHoldDuration > 0f) yield return new WaitForSecondsRealtime(blackHoldDuration);
        onBlack?.Invoke();
        yield return null; // let the change take effect before revealing
        if (revealDelayDuration > 0f) yield return new WaitForSecondsRealtime(revealDelayDuration);

        yield return Fade(1f, 0f, fadeOutDuration);
        fadeGroup.blocksRaycasts = false;
        fadeGroup.gameObject.SetActive(false);
        loading = false;

        onDone?.Invoke();
    }

    private IEnumerator Load(string sceneName)
    {
        loading = true;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (op == null)
        {
            Debug.LogError($"SceneLoader: could not start loading '{sceneName}'. Is it in Build Settings?", this);
            loading = false;
            yield break;
        }

        // Preload while fading to black, then activate the new scene on a fully black screen.
        op.allowSceneActivation = false;
        EnsureFadeOverlay();
        fadeGroup.blocksRaycasts = true;
        yield return Fade(0f, 1f, fadeInDuration);

        while (op.progress < 0.9f && !op.isDone) yield return null;
        if (blackHoldDuration > 0f) yield return new WaitForSecondsRealtime(blackHoldDuration);

        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;
        yield return null; // let the new scene's Start run before revealing it
        if (revealDelayDuration > 0f) yield return new WaitForSecondsRealtime(revealDelayDuration);

        yield return Fade(1f, 0f, fadeOutDuration);
        fadeGroup.blocksRaycasts = false;
        fadeGroup.gameObject.SetActive(false);
        loading = false;
    }

    /// Creates an overlay canvas that lives on this (scene-independent) loader object.
    private void EnsureFadeOverlay()
    {
        if (fadeGroup != null) return;

        GameObject canvasObject = new("SceneFade", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject imageObject = new("Black", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rect = (RectTransform)imageObject.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        imageObject.GetComponent<Image>().color = Color.black;

        fadeGroup = canvasObject.GetComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.interactable = false;
        fadeGroup.blocksRaycasts = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        fadeGroup.gameObject.SetActive(true);

        if (duration <= 0f)
        {
            fadeGroup.alpha = to;
            yield break;
        }

        fadeGroup.alpha = from;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // A scene-load hitch can produce a huge delta; cap it so the fade takes several frames.
            elapsed += Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
            fadeGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        fadeGroup.alpha = to;
    }
}
