using TMPro;
using UnityEngine;

public class TargetScoreText : MonoBehaviour
{
    [SerializeField] private EncounterManager encounterManager;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI resultLabel;   // optional: "VICTORY" / "DEFEAT"

    void OnEnable()  => encounterManager.EncounterEnded += OnEncounterEnded;
    void OnDisable() => encounterManager.EncounterEnded -= OnEncounterEnded;

    void Start()
    {
        label.text = $"Goal: {encounterManager.TargetScore}";
        if (resultLabel != null) resultLabel.text = string.Empty;
    }

    private void OnEncounterEnded(bool won)
    {
        if (resultLabel != null) resultLabel.text = won ? "VICTORY" : "DEFEAT";
    }
}