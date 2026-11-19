using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Colors every piece of the combo star by its current status.
/// Each piece can be a UI Graphic (TextMeshProUGUI, Image) and/or a SpriteRenderer.
public class ComboStarView : MonoBehaviour
{
    [Serializable]
    public class Piece
    {
        public CardValue value;
        public Graphic uiGraphic;             // TMP number or UI Image
        public SpriteRenderer spriteRenderer; // alternative for world-space symbols
    }

    [Header("References")]
    [SerializeField] private ComboStarManager starManager;
    [SerializeField] private List<Piece> pieces = new List<Piece>();

    [Header("Colors")]
    [SerializeField] private Color noneColor = Color.white;
    [SerializeField] private Color sequenceColor = new Color(0.25f, 0.85f, 0.35f, 1f);
    [SerializeField] private Color tripletColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    [Header("Animation")]
    [SerializeField] private float colorDuration = 0.2f;
    [SerializeField] private float punchStrength = 0.3f;
    [SerializeField] private float punchDuration = 0.35f;

    private readonly Dictionary<CardValue, Piece> lookup = new Dictionary<CardValue, Piece>();

    void Awake()
    {
        foreach (Piece piece in pieces)
            lookup[piece.value] = piece;
    }

    void OnEnable()
    {
        starManager.PieceChanged += OnPieceChanged;
        starManager.TurnReset += RefreshAll;
    }

    void OnDisable()
    {
        starManager.PieceChanged -= OnPieceChanged;
        starManager.TurnReset -= RefreshAll;
    }

    void Start() => RefreshAll();

    private void OnPieceChanged(CardValue value, ComboActiveStatus status)
    {
        if (!lookup.TryGetValue(value, out Piece piece)) return;
        Apply(piece, status, animate: true);
    }

    private void RefreshAll()
    {
        foreach (Piece piece in pieces)
            Apply(piece, starManager.GetStatus(piece.value), animate: false);
    }

    private void Apply(Piece piece, ComboActiveStatus status, bool animate)
    {
        Color color = ColorFor(status);

        if (piece.uiGraphic != null)
        {
            piece.uiGraphic.DOKill();
            if (animate) piece.uiGraphic.DOColor(color, colorDuration).SetLink(piece.uiGraphic.gameObject);
            else piece.uiGraphic.color = color;
            if (animate) Punch(piece.uiGraphic.transform);
        }

        if (piece.spriteRenderer != null)
        {
            piece.spriteRenderer.DOKill();
            if (animate) piece.spriteRenderer.DOColor(color, colorDuration).SetLink(piece.spriteRenderer.gameObject);
            else piece.spriteRenderer.color = color;
            if (animate && piece.uiGraphic == null) Punch(piece.spriteRenderer.transform);
        }
    }

    private void Punch(Transform t)
    {
        t.DOComplete();   // finish a running punch so the scale resets cleanly
        t.DOPunchScale(Vector3.one * punchStrength, punchDuration, 8, 0.5f)
         .SetLink(t.gameObject);
    }

    private Color ColorFor(ComboActiveStatus status) => status switch
    {
        ComboActiveStatus.ActiveBySequence => sequenceColor,
        ComboActiveStatus.ActiveByTriplet => tripletColor,
        ComboActiveStatus.Disabled => disabledColor,
        _ => noneColor
    };

#if UNITY_EDITOR
    /// Right-click the component header -> fills the list from child objects named
    /// "1".."6" (Bamboo1..6) and "Earth", "Fire", "Water", "Wind".
    [ContextMenu("Auto-fill pieces from children")]
    private void AutoFill()
    {
        pieces.Clear();

        foreach (CardValue value in Enum.GetValues(typeof(CardValue)))
        {
            string childName = value <= CardValue.Bamboo6
                ? ((int)value + 1).ToString()
                : value.ToString();

            Transform child = transform.Find(childName);
            if (child == null) continue;

            pieces.Add(new Piece
            {
                value = value,
                uiGraphic = child.GetComponent<Graphic>(),
                spriteRenderer = child.GetComponent<SpriteRenderer>()
            });
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"ComboStarView: found {pieces.Count}/10 pieces.", this);
    }
#endif
}