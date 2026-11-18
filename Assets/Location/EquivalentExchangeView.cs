using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// Small overlay inside MapScene. Built entirely in code (no prefab needed).
/// Flow: Show() -> fade to black -> view appears -> fade in -> button -> fade to black -> view hidden -> fade in.
public class EquivalentExchangeView : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private CardPool pool;
    [SerializeField] private string titleText = "Test String";
    [SerializeField] private string buttonText = "Continue";

    [Header("Look")]
    [SerializeField] private Vector2 panelSize = new Vector2(520f, 640f);
    [SerializeField] private Vector2 cardSize = new Vector2(260f, 360f);
    [SerializeField] private Color dimColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private Color panelColor = new Color(0.08f, 0.08f, 0.12f, 0.98f);

    // Lower than SceneLoader's fade canvas (short.MaxValue) so the fade stays on top
    private const int CanvasSortingOrder = 100;
    private const int Salt = 0x0E0E;

    private GameObject root;
    private Image cardImage;
    private TextMeshProUGUI cardNameLabel;
    private bool busy;
    private Action onClosed;

    public bool IsOpen { get; private set; }

    void Awake()
    {
        EnsureEventSystem();
        Build();
        root.SetActive(false);
    }

    // ---------- Public API ----------

    /// Opens the view with a fade. onClosed fires after the closing fade has finished.
    public void Show(RunState state, Action onClosed)
    {
        if (IsOpen) return;

        IsOpen = true;
        busy = true;
        this.onClosed = onClosed;

        CardBase card = RollCard(state);

        bool started = SceneLoader.Instance.Transition(
            onBlack: () => Present(card),
            onDone: () => busy = false);

        if (!started)   // loader was busy: skip the fade rather than getting stuck
        {
            Present(card);
            busy = false;
        }
    }

    // ---------- Logic ----------

    private CardBase RollCard(RunState state)
    {
        if (pool == null)
        {
            Debug.LogError("EquivalentExchangeView: no CardPool assigned.", this);
            return null;
        }

        // Own rng per node: same run + same node => same card, regardless of how much RunManager.Rng was used.
        var rng = new System.Random(DeriveSeed(state.Seed, state.CurrentNodeId));
        return pool.Pick(rng);
    }

    private static int DeriveSeed(int runSeed, int nodeId)
    {
        // Don't use HashCode.Combine: it is randomized per process, so it isn't reproducible.
        unchecked
        {
            return (runSeed * 73856093) ^ (nodeId * 19349663) ^ (Salt * 83492791);
        }
    }

    private void Present(CardBase card)
    {
        Sprite art = card != null ? card.art : null;
        cardImage.sprite = art;
        cardImage.enabled = art != null;
        cardNameLabel.text = card != null ? card.name : string.Empty;

        root.SetActive(true);
    }

    private void OnButtonClicked()
    {
        if (busy) return;
        busy = true;

        // TODO: apply the exchange here (e.g. add the card to RunManager.Instance.State.Deck).

        bool started = SceneLoader.Instance.Transition(onBlack: Hide, onDone: Finish);
        if (!started)
        {
            Hide();
            Finish();
        }
    }

    private void Hide() => root.SetActive(false);

    private void Finish()
    {
        busy = false;
        IsOpen = false;
        onClosed?.Invoke();
    }

    // ---------- UI construction ----------

    private void Build()
    {
        root = new GameObject("ExchangeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = CanvasSortingOrder;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Dim background (also swallows UI clicks outside the panel)
        RectTransform dim = NewRect("Dim", root.transform);
        Stretch(dim);
        dim.gameObject.AddComponent<Image>().color = dimColor;

        // Panel
        RectTransform panel = NewRect("Panel", root.transform);
        Place(panel, new Vector2(0.5f, 0.5f), Vector2.zero, panelSize);
        panel.gameObject.AddComponent<Image>().color = panelColor;

        // Title (top)
        TextMeshProUGUI title = NewText("Title", panel, titleText, 48f);
        Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(480f, 70f));

        // Card art (middle)
        RectTransform card = NewRect("Card", panel);
        Place(card, new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), cardSize);
        cardImage = card.gameObject.AddComponent<Image>();
        cardImage.preserveAspect = true;
        cardImage.raycastTarget = false;

        // Card name
        cardNameLabel = NewText("CardName", panel, string.Empty, 32f);
        Place(cardNameLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 115f), new Vector2(480f, 45f));

        // Button (bottom)
        RectTransform buttonRect = NewRect("Button", panel);
        Place(buttonRect, new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(240f, 60f));
        Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(0.25f, 0.45f, 0.85f, 1f);
        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(OnButtonClicked);

        TextMeshProUGUI buttonLabel = NewText("Label", buttonRect, buttonText, 30f);
        Stretch(buttonLabel.rectTransform);
    }

    private static void EnsureEventSystem()
    {
        // MapScene has no EventSystem, and buttons need one.
        if (FindAnyObjectByType<EventSystem>() != null) return;

        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, string text, float size)
    {
        RectTransform rt = NewRect(name, parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.raycastTarget = false;
        return t;
    }

    /// Anchor and pivot are the same point, so pos is an offset from that edge/corner/center.
    private static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
