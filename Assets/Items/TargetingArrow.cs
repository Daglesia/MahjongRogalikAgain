using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// Curved arrow from ArrowStart to the mouse cursor. Shown while an item waits for a card target.
/// Clings to the center of a valid target card when the cursor is over it.
[RequireComponent(typeof(LineRenderer))]
public class TargetingArrow : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The scene object the arrow starts from (ArrowStart). Falls back to this object.")]
    [SerializeField] private Transform arrowStart;
    [SerializeField] private Camera cam;
    [Tooltip("Optional. Assets/Shaders/MapLine.mat works. A URP 2D sprite material is created if empty.")]
    [SerializeField] private Material lineMaterial;
    [Tooltip("Optional. Must point RIGHT with its pivot at the TIP. A triangle is generated if empty.")]
    [SerializeField] private Sprite headSprite;

    [Header("Curve")]
    [Tooltip("Sideways bow of the curve, as a fraction of the start-end distance. 0 = straight line, negative = other side.")]
    [SerializeField, Range(-1.5f, 1.5f)] private float bend = 0.35f;
    [Tooltip("Always bow upwards regardless of where the cursor is. Off = bow to one side of the direction of travel.")]
    [SerializeField] private bool bendUpwards = true;
    [Tooltip("Extra world-space lift added to the curve (units, always up).")]
    [SerializeField] private float lift = 0f;
    [Tooltip("How far along the start->end line the first control point sits (0-1).")]
    [SerializeField, Range(0f, 1f)] private float startHandle = 0.25f;
    [Tooltip("How far back from the end the second control point sits (0-1).")]
    [SerializeField, Range(0f, 1f)] private float endHandle = 0.25f;
    [SerializeField, Range(4, 64)] private int segments = 28;

    [Header("Cling")]
    [Tooltip("How fast the tip glides to the card center / back to the cursor. Higher = snappier.")]
    [SerializeField, Min(1f)] private float clingSpeed = 25f;

    [Header("Look")]
    [SerializeField] private Color color = new Color(1f, 0.85f, 0.2f, 1f);
    [Tooltip("Color while clinging to a valid target. Set equal to Color to disable the change.")]
    [SerializeField] private Color snappedColor = new Color(0.3f, 1f, 0.4f, 1f);
    [SerializeField, Min(0.001f)] private float lineWidth = 0.12f;
    [Tooltip("Width along the shaft (0 = start, 1 = end).")]
    [SerializeField] private AnimationCurve widthCurve = AnimationCurve.Linear(0f, 0.6f, 1f, 1f);
    [SerializeField, Min(0.01f)] private float headLength = 0.45f;
    [SerializeField] private string sortingLayerName = "Cards";
    [SerializeField] private int sortingOrder = 50;

    private LineRenderer line;
    private Transform head;
    private SpriteRenderer headRenderer;
    private Vector3[] points;
    private bool visible;

    private Func<Card, bool> isValidTarget;
    private Vector3 currentEnd;
    private bool hasEnd;
    private bool snapped;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.numCapVertices = 0;
        line.textureMode = LineTextureMode.Stretch;

        var headGo = new GameObject("ArrowHead");
        head = headGo.transform;
        head.SetParent(transform, false);
        headRenderer = headGo.AddComponent<SpriteRenderer>();

        SetVisible(false);
    }

    /// validTarget decides which cards the arrow may cling to (null = never cling).
    public void Show(Func<Card, bool> validTarget = null)
    {
        if (cam == null) cam = Camera.main;
        isValidTarget = validTarget;
        hasEnd = false;      // start from the cursor, don't glide in from the last position
        snapped = false;

        ApplyLook();
        SetVisible(true);
        UpdateArrow();
    }

    public void Hide()
    {
        isValidTarget = null;
        hasEnd = false;
        SetVisible(false);
    }

    void LateUpdate()
    {
        if (visible) UpdateArrow();
    }

    void OnValidate()
    {
        if (line != null) ApplyLook();
    }

    // ---------- Drawing ----------

    private void UpdateArrow()
    {
        if (Mouse.current == null || cam == null) return;

        Vector3 cursor = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        cursor.z = 0f;

        // Where should the tip go? Card center if hovering a valid target, otherwise the cursor.
        Vector3 desired = cursor;
        bool nowSnapped = false;

        if (isValidTarget != null)
        {
            foreach (Collider2D hit in Physics2D.OverlapPointAll(cursor))
            {
                if (hit.TryGetComponent<Card>(out var card) && isValidTarget(card))
                {
                    desired = hit.bounds.center;
                    nowSnapped = true;
                    break;
                }
            }
        }

        if (nowSnapped != snapped)
        {
            snapped = nowSnapped;
            ApplyColor(snapped ? snappedColor : color);
        }

        if (!hasEnd)
        {
            currentEnd = desired;
            hasEnd = true;
        }
        else
        {
            currentEnd = Vector3.Lerp(currentEnd, desired, 1f - Mathf.Exp(-clingSpeed * Time.unscaledDeltaTime));
        }
        currentEnd.z = 0f;

        Vector3 p0 = (arrowStart != null ? arrowStart : transform).position;
        p0.z = 0f;
        Vector3 p3 = currentEnd;

        Vector3 delta = p3 - p0;
        float dist = delta.magnitude;
        if (dist < 0.0001f) return;
        Vector3 dir = delta / dist;

        // Perpendicular to the travel direction
        Vector3 normal = new Vector3(-dir.y, dir.x, 0f);
        if (bendUpwards && normal.y < 0f) normal = -normal;

        Vector3 offset = normal * (bend * dist) + Vector3.up * lift;
        Vector3 p1 = p0 + dir * (dist * startHandle) + offset;
        Vector3 p2 = p3 - dir * (dist * endHandle) + offset;

        int n = segments + 1;
        if (points == null || points.Length != n) points = new Vector3[n];
        for (int i = 0; i < n; i++)
            points[i] = Bezier(p0, p1, p2, p3, i / (float)segments);

        // Walk back from the tip to find where the shaft should stop (so it doesn't poke through the head)
        float remaining = headLength;
        int cut = n - 1;
        Vector3 shaftEnd = points[n - 1];
        for (int i = n - 1; i > 0; i--)
        {
            float seg = Vector3.Distance(points[i], points[i - 1]);
            if (seg >= remaining)
            {
                shaftEnd = Vector3.Lerp(points[i], points[i - 1], remaining / seg);
                cut = i - 1;
                break;
            }
            remaining -= seg;
            cut = i - 1;
            shaftEnd = points[i - 1];
        }

        line.positionCount = cut + 2;
        for (int i = 0; i <= cut; i++) line.SetPosition(i, points[i]);
        line.SetPosition(cut + 1, shaftEnd);

        Vector3 tangent = points[n - 1] - shaftEnd;
        if (tangent.sqrMagnitude < 1e-8f) tangent = dir;
        float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
        head.SetPositionAndRotation(points[n - 1], Quaternion.Euler(0f, 0f, angle));
    }

    private static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1f - t;
        return u * u * u * a + 3f * u * u * t * b + 3f * u * t * t * c + t * t * t * d;
    }

    // ---------- Look ----------

    private void ApplyLook()
    {
        if (lineMaterial == null)
        {
            Shader s = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (s != null) lineMaterial = new Material(s);
        }
        if (lineMaterial != null) line.sharedMaterial = lineMaterial;

        line.widthCurve = widthCurve;
        line.widthMultiplier = lineWidth;
        line.sortingLayerName = sortingLayerName;
        line.sortingOrder = sortingOrder;

        if (headRenderer == null) return;

        if (headSprite == null) headSprite = CreateTriangleSprite();
        headRenderer.sprite = headSprite;
        headRenderer.sortingLayerName = sortingLayerName;
        headRenderer.sortingOrder = sortingOrder;

        float scale = headLength / headSprite.bounds.size.x;
        head.localScale = new Vector3(scale, scale, 1f);

        ApplyColor(snapped ? snappedColor : color);
    }

    private void ApplyColor(Color c)
    {
        line.startColor = line.endColor = c;
        if (headRenderer != null) headRenderer.color = c;
    }

    private void SetVisible(bool value)
    {
        visible = value;
        line.enabled = value;
        if (headRenderer != null) headRenderer.enabled = value;
    }

    /// Triangle pointing right, pivot at the tip.
    private static Sprite CreateTriangleSprite()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        var px = new Color[size * size];
        float c = size / 2f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float halfHeight = c * (1f - x / (float)size);
                px[y * size + x] = Mathf.Abs(y + 0.5f - c) <= halfHeight ? Color.white : Color.clear;
            }

        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(1f, 0.5f), size);
    }
}