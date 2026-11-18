using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EndTurnButton : CardColleague
{
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private TextMeshProUGUI label;   // optional

    void Awake() => GetComponent<Button>().onClick.AddListener(OnClicked);

    void OnEnable()  => turnManager.TurnChanged += Refresh;
    void OnDisable() => turnManager.TurnChanged -= Refresh;

    void Start()
    {
        Mediator = FindAnyObjectByType<CardMediator>();
        Refresh(turnManager.CurrentTurn, turnManager.MaxTurns);
    }

    private void OnClicked() => Mediator?.NotifyEndTurnRequested();

    private void Refresh(int turn, int max)
    {
        if (label != null) label.text = $"End Turn ({turn}/{max})";
    }
}