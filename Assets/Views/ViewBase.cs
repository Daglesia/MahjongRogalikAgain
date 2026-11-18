using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// Base for every non-combat screen (shop, exchange, ...).
/// The prefab root holds a Canvas + this component; "content" is the child that gets shown/hidden.
public abstract class ViewBase : MonoBehaviour, IView
{
    [Header("View")]
    [Tooltip("Child object that is toggled on open/close. Must NOT be the object holding this script.")]
    [SerializeField] private GameObject content;

    private Action onClosed;
    private bool busy;

    protected RunState State { get; private set; }
    protected bool IsBusy => busy;
    public bool IsOpen { get; private set; }

    protected virtual void Awake()
    {
        if (content == null)
            Debug.LogError($"{GetType().Name}: 'Content' is not assigned on the prefab.", this);

        EnsureEventSystem();
        SetContentActive(false);
    }

    // ---------- IView ----------

    public void Open(RunState state, Action onClosed = null)
    {
        if (IsOpen) return;

        IsOpen = true;
        busy = true;
        State = state;
        this.onClosed = onClosed;

        bool started = SceneLoader.Instance.Transition(
            onBlack: ShowContent,
            onDone: () => busy = false);

        if (!started)   // loader was busy: skip the fade rather than getting stuck
        {
            ShowContent();
            busy = false;
        }
    }

    public void Close()
    {
        if (!IsOpen || busy) return;
        busy = true;

        bool started = SceneLoader.Instance.Transition(onBlack: HideContent, onDone: Finish);
        if (!started)
        {
            HideContent();
            Finish();
        }
    }

    public virtual void UpdateDeck(RunState state) { }

    // ---------- Hooks for subclasses ----------

    /// Called while the screen is fully black, right before the content becomes visible.
    protected abstract void OnOpen(RunState state);

    /// Called while the screen is fully black, right before the content is hidden.
    protected virtual void OnClose() { }

    // ---------- Internals ----------

    private void ShowContent()
    {
        OnOpen(State);
        SetContentActive(true);
    }

    private void HideContent()
    {
        OnClose();
        SetContentActive(false);
    }

    private void Finish()
    {
        busy = false;
        IsOpen = false;

        Action callback = onClosed;
        onClosed = null;
        callback?.Invoke();
    }

    private void SetContentActive(bool value)
    {
        if (content != null) content.SetActive(value);
    }

    private static void EnsureEventSystem()
    {
        // MapScene may have no EventSystem, and buttons need one.
        if (FindAnyObjectByType<EventSystem>() != null) return;

        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }
}