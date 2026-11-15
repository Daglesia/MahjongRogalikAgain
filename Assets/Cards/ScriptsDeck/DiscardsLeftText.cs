using TMPro;
using UnityEngine;

public class DiscardsLeftText : MonoBehaviour
{
    [SerializeField] private DiscardManager discardManager;
    [SerializeField] private TextMeshProUGUI label;

    void OnEnable()  => discardManager.DiscardsChanged += OnDiscardsChanged;
    void OnDisable() => discardManager.DiscardsChanged -= OnDiscardsChanged;

    void Start() => OnDiscardsChanged(discardManager.Count);

    private void OnDiscardsChanged(int discardsLeft)
    {
        label.text = discardsLeft.ToString();
    }
}