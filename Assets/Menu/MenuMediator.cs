using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class MenuMediator : MonoBehaviour, IMenuMediator
{
    private bool starting;

    void Awake() => EnsureEventSystem();

    public void NotifyNewRunClicked()
    {
        if (starting) return;
        starting = true;

        // RunManager lives in this scene (with the starting deck assigned) and survives scene loads.
        RunManager.Instance.StartNewRun();
        SceneLoader.Instance.LoadMap();
    }

    public void NotifySettingsClicked()
    {
        Debug.Log("Menu: Settings clicked");
    }

    public void NotifyExitClicked()
    {
        Debug.Log("Menu: Exit clicked");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void EnsureEventSystem()
    {
        // Buttons need an EventSystem; same approach as ViewBase.
        if (FindAnyObjectByType<EventSystem>() != null) return;

        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }
}