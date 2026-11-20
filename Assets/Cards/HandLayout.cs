using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// Flexbox-like horizontal layout for world-space children (SpriteRenderer cards).
public class HandLayout : MonoBehaviour
{
    public enum Justify { Start, Center, End }

    [Header("Layout")]
    [SerializeField] private Justify justify = Justify.Center;
    [SerializeField] private float gap = 0.225f;       // space between cards (local units)
    [SerializeField] private float maxWidth = 0f;      // 0 = unlimited; otherwise gap shrinks (cards overlap) to fit

    [Header("Animation")]
    [SerializeField] private bool animate = true;
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private Ease ease = Ease.OutQuad;

    private readonly Dictionary<Transform, Tween> tweens = new Dictionary<Transform, Tween>();
    private readonly List<Transform> items = new List<Transform>();
    private readonly List<float> widths = new List<float>();
    private bool dirty = true;
    private bool firstLayout = true;

    void OnTransformChildrenChanged() => dirty = true;
    void OnValidate() => dirty = true;

    void LateUpdate()
    {
        if (!dirty) return;
        dirty = false;
        Relayout(instant: firstLayout || !animate);
        firstLayout = false;
    }

    public void MarkDirty() => dirty = true;

    public void Relayout(bool instant = false)
    {
        KillTweensOfLeftChildren();

        items.Clear();
        widths.Clear();
        float sumWidths = 0f;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            float w = GetItemWidth(child);
            items.Add(child);
            widths.Add(w);
            sumWidths += w;
        }

        int n = items.Count;
        if (n == 0) return;

        float usedGap = gap;
        float total = sumWidths + gap * (n - 1);

        if (maxWidth > 0f && total > maxWidth && n > 1)
        {
            usedGap = (maxWidth - sumWidths) / (n - 1); // may go negative => overlap
            total = maxWidth;
        }

        float cursor = justify switch
        {
            Justify.Center => -total * 0.5f,
            Justify.End => -total,
            _ => 0f
        };

        for (int i = 0; i < n; i++)
        {
            float centerX = cursor + widths[i] * 0.5f;
            Place(items[i], centerX, instant);
            cursor += widths[i] + usedGap;
        }
    }

    private void Place(Transform t, float x, bool instant)
    {
        Vector3 target = new Vector3(x, 0f, t.localPosition.z);

        if (t.TryGetComponent<Card>(out var card))
            card.SetBasePosition(transform.TransformPoint(target));

        if (tweens.TryGetValue(t, out var old)) old.Kill();

        if (instant || moveDuration <= 0f)
        {
            t.localPosition = target;
            return;
        }

        tweens[t] = DOTween.To(() => t.localPosition, v => t.localPosition = v, target, moveDuration)
                           .SetEase(ease)
                           .SetLink(t.gameObject)
                           .OnKill(() => tweens.Remove(t));
    }

    // Width in the Hand's local space = sprite size * child's local scale.
    // For cards use BaseScale, so a hover scale doesn't shift neighbours on relayout.
    private static float GetItemWidth(Transform child)
    {
        float scaleX = child.TryGetComponent<Card>(out var c)
            ? Mathf.Abs(c.BaseScale.x)
            : Mathf.Abs(child.localScale.x);

        if (child.TryGetComponent<SpriteRenderer>(out var sr) && sr.sprite != null)
            return sr.sprite.bounds.size.x * scaleX;
        return scaleX;
    }

    // Cards that flew away (unparented) must not keep being moved by the layout
    private void KillTweensOfLeftChildren()
    {
        List<Transform> toKill = null;
        foreach (var kv in tweens)
        {
            if (kv.Key == null || kv.Key.parent != transform)
                (toKill ??= new List<Transform>()).Add(kv.Key);
        }
        if (toKill == null) return;
        foreach (var t in toKill)
            if (tweens.TryGetValue(t, out var tw)) tw.Kill();
    }
}
