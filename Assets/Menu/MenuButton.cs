using UnityEngine;
using UnityEngine.UI;

/// One component for every menu button. It only reports WHAT was clicked;
/// the MenuMediator decides what happens.
[RequireComponent(typeof(Button))]
public class MenuButton : MenuColleague
{
    public enum Kind { NewRun, Settings, Exit }

    [SerializeField] private Kind kind;

    void Awake() => GetComponent<Button>().onClick.AddListener(OnClicked);

    void Start() => Mediator = FindAnyObjectByType<MenuMediator>();

    private void OnClicked()
    {
        if (Mediator == null) return;

        switch (kind)
        {
            case Kind.NewRun:   Mediator.NotifyNewRunClicked();   break;
            case Kind.Settings: Mediator.NotifySettingsClicked(); break;
            case Kind.Exit:     Mediator.NotifyExitClicked();     break;
        }
    }
}