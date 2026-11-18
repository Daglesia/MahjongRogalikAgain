using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Combat overlay that lists every card still in the draw pile.
/// It is part of CombatScene - opening and closing it never loads another scene.
public class DeckView : ViewBase
{
    private const int Columns = 8;
    private const float CardWidth = 170f;
    private const float CardHeight = 225f;

    private static DeckView activeView;

    private DeckManager deckManager;
    private TextMeshProUGUI countLabel;
    private RectTransform cardGrid;
    private Button backButton;
    private readonly List<GameObject> cardTiles = new List<GameObject>();

    /// Used by CardMediator to ignore world-space card clicks while this overlay is up.
    public static bool AnyOpen => activeView != null && activeView.IsOpen;

    protected override void Awake()
    {
        base.Awake();
        activeView = this;

        deckManager = FindAnyObjectByType<DeckManager>();
        if (deckManager == null)
            Debug.LogError("DeckView: no DeckManager found in the scene.", this);

        BuildUi();
    }

    private void OnDestroy()
    {
        if (activeView == this) activeView = null;
    }

    /// Convenience entry point for the RestOfDeck click. The state is not used by this view.
    public void Show() => Open(null, null);

    // ---------- ViewBase ----------

    protected override void OnOpen(RunState state)
    {
        RefreshCards();
    }

    // ---------- UI ----------

    private void BuildUi()
    {
        Transform content = transform.Find("Content");
        if (content == null)
        {
            Debug.LogError("DeckView: 'Content' child is missing.", this);
            return;
        }

        Stretch((RectTransform)content);

        GameObject backdrop = CreateUiObject("Backdrop", content, typeof(Image));
        Stretch((RectTransform)backdrop.transform);
        backdrop.GetComponent<Image>().color = new Color(0.04f, 0.04f, 0.06f, 0.94f);

        GameObject title = CreateUiObject("Title", content, typeof(TextMeshProUGUI));
        SetRect((RectTransform)title.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 1f), new Vector2(48f, -42f), new Vector2(500f, 70f));
        SetText(title.GetComponent<TextMeshProUGUI>(), "Deck", 46f, TextAlignmentOptions.Left);

        GameObject count = CreateUiObject("Count", content, typeof(TextMeshProUGUI));
        SetRect((RectTransform)count.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 1f), new Vector2(50f, -112f), new Vector2(700f, 50f));
        countLabel = count.GetComponent<TextMeshProUGUI>();
        SetText(countLabel, "Cards left: 0", 30f, TextAlignmentOptions.Left);

        GameObject back = CreateUiObject("BackButton", content, typeof(Image), typeof(Button));
        SetRect((RectTransform)back.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(1f, 1f), new Vector2(-48f, -42f), new Vector2(170f, 64f));
        back.GetComponent<Image>().color = new Color(0.24f, 0.24f, 0.3f, 1f);
        backButton = back.GetComponent<Button>();
        backButton.onClick.AddListener(OnBackClicked);

        GameObject backLabel = CreateUiObject("Label", back.transform, typeof(TextMeshProUGUI));
        Stretch((RectTransform)backLabel.transform);
        SetText(backLabel.GetComponent<TextMeshProUGUI>(), "Back", 30f, TextAlignmentOptions.Center);

        GameObject scroll = CreateUiObject("ScrollArea", content, typeof(ScrollRect));
        SetOffsets((RectTransform)scroll.transform, new Vector2(48f, 110f), new Vector2(-48f, -175f));

        GameObject viewport = CreateUiObject("Viewport", scroll.transform, typeof(Image), typeof(Mask));
        Stretch((RectTransform)viewport.transform);
        viewport.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.11f, 0.7f);
        viewport.GetComponent<Mask>().showMaskGraphic = true;

        GameObject grid = CreateUiObject("Grid", viewport.transform, typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        RectTransform gridRect = (RectTransform)grid.transform;
        gridRect.anchorMin = new Vector2(0f, 1f);
        gridRect.anchorMax = new Vector2(1f, 1f);
        gridRect.pivot = new Vector2(0.5f, 1f);
        gridRect.anchoredPosition = Vector2.zero;
        gridRect.sizeDelta = Vector2.zero;

        GridLayoutGroup layout = grid.GetComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(CardWidth, CardHeight);
        layout.spacing = new Vector2(14f, 14f);
        layout.padding = new RectOffset(16, 16, 16, 16);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = Columns;
        layout.startAxis = GridLayoutGroup.Axis.Horizontal;

        ContentSizeFitter fitter = grid.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scroll.GetComponent<ScrollRect>();
        scrollRect.viewport = (RectTransform)viewport.transform;
        scrollRect.content = gridRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        cardGrid = gridRect;
    }

    private void RefreshCards()
    {
        foreach (GameObject tile in cardTiles)
        {
            if (tile != null) Destroy(tile);
        }
        cardTiles.Clear();

        IReadOnlyList<CardBase> cards = deckManager != null
            ? deckManager.GetRemainingCardsSorted()
            : Array.Empty<CardBase>();

        countLabel.text = $"Cards left: {cards.Count}";

        foreach (CardBase card in cards)
            cardTiles.Add(CreateCardTile(card));
    }

    private GameObject CreateCardTile(CardBase card)
    {
        GameObject tile = CreateUiObject(card != null ? card.name : "Card", cardGrid, typeof(Image));
        tile.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

        GameObject art = CreateUiObject("Art", tile.transform, typeof(Image));
        RectTransform artRect = (RectTransform)art.transform;
        artRect.anchorMin = new Vector2(0.5f, 1f);
        artRect.anchorMax = new Vector2(0.5f, 1f);
        artRect.pivot = new Vector2(0.5f, 1f);
        artRect.anchoredPosition = new Vector2(0f, -10f);
        artRect.sizeDelta = new Vector2(CardWidth - 24f, CardHeight - 82f);

        Image artImage = art.GetComponent<Image>();
        artImage.sprite = card != null ? card.art : null;
        artImage.preserveAspect = true;
        artImage.color = artImage.sprite != null ? Color.white : new Color(0.28f, 0.28f, 0.34f, 1f);

        GameObject label = CreateUiObject("Name", tile.transform, typeof(TextMeshProUGUI));
        RectTransform labelRect = (RectTransform)label.transform;
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.anchoredPosition = new Vector2(0f, 12f);
        labelRect.sizeDelta = new Vector2(-16f, 50f);

        TextMeshProUGUI nameLabel = label.GetComponent<TextMeshProUGUI>();
        SetText(nameLabel, card != null ? card.name : string.Empty, 22f, TextAlignmentOptions.Center);
        nameLabel.textWrappingMode = TextWrappingModes.NoWrap;
        nameLabel.overflowMode = TextOverflowModes.Ellipsis;

        return tile;
    }

    private void OnBackClicked()
    {
        if (IsBusy) return;
        Close();   // only hides the overlay - CombatScene stays loaded
    }

    // ---------- Helpers ----------

    private static GameObject CreateUiObject(string name, Transform parent, params Type[] components)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        foreach (Type component in components)
            go.AddComponent(component);

        return go;
    }

    private static void SetText(TextMeshProUGUI text, string value, float size, TextAlignmentOptions alignment)
    {
        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetOffsets(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = min;
        rect.offsetMax = max;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }
}
