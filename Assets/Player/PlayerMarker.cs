using System;
using DG.Tweening;
using UnityEngine;

public class PlayerMarker : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new Vector2(0f, 0.6f);
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private Ease ease = Ease.InOutQuad;

    public void PlaceAt(Vector2 nodePosition)
    {
        transform.DOKill();
        transform.position = (Vector3)(nodePosition + offset);
    }

    public void MoveTo(Vector2 nodePosition, Action onArrived)
    {
        transform.DOKill();
        transform.DOMove((Vector3)(nodePosition + offset), moveDuration)
                 .SetEase(ease)
                 .SetLink(gameObject)
                 .OnComplete(() => onArrived?.Invoke());
    }
}