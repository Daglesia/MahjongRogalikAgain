using UnityEngine;

/// Clicking the rest-of-deck pile in combat opens the in-scene DeckView overlay.
[RequireComponent(typeof(Collider2D))]
public class RestOfDeckClick : MonoBehaviour
{
    private DeckView deckView;

    private void Awake()
    {
        deckView = FindAnyObjectByType<DeckView>();
        if (deckView == null)
            Debug.LogError("RestOfDeckClick: no DeckView found in the scene.", this);
    }

    private void OnMouseDown()
    {
        if (deckView == null || deckView.IsOpen) return;   // instance property from ViewBase
        deckView.Show();
    }
}
