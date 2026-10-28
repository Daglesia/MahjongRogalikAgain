using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private ComboManager comboManager;
    [SerializeField] private CardDefinition cardDefinition;

    void OnMouseDown()
    {
        comboManager.AddCardToCombo(cardDefinition.GetCard());
    }
}