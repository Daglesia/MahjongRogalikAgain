using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Pause overlay (Continue / Settings). Built in code, no prefab needed.
/// Opens instantly (no scene fade) so the game can't keep running while it fades in.
public class PauseView : ViewBase
{
    private static PauseView activeView;

    /// Used by mediators to ignore world-space (OnMouseDown) clicks while paused.
    public static bool AnyOpen => activeView != null && activeView.IsOpen;

    protected override bool UseTransition => false;

    protected override void Awake()
    {
        activeView = this;
        BuildUi();          // must run first: ViewBase.Awake needs "content"
        base.Awake();
    }

    private void OnDestroy()
    {
        if (activeView == this) activeView = null;
    }

    // ---------- ViewBase ----------

    protected override void OnOpen(RunState state) { }

    // ---------- UI ----------

    private void BuildUi()
    {
        // This object is the canvas root (same layout as the other view prefabs)
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;   // above other views (100)

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        GameObject content = CreateUiObject("Content", transform);
        Stretch((RectTransform)content.transform);
        SetContent(content);

        // Dim background (blocks clicks on the UI underneath)
        GameObject dim = CreateUiObject("Dim", content.transform, typeof(Image));
        Stretch((RectTransform)dim.transform);
        dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        // Panel
        GameObject panel = CreateUiObject("Panel", content.transform, typeof(Image));
        var panelRect = (RectTransform)panel.transform;
        panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(520f, 440f);
        panel.GetComponent<Image>().color = new Color(0.078f, 0.078f, 0.122f, 0.98f);

        // Title
        GameObject title = CreateUiObject("Title", panel.transform, typeof(TextMeshProUGUI));
        var titleRect = (RectTransform)title.transform;
        titleRect.anchorMin = titleRect.anchorMax = titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -40f);
        titleRect.sizeDelta = new Vector2(480f, 80f);
        SetText(title.GetComponent<TextMeshProUGUI>(), "Paused", 56f);

        // Buttons
        GameObject buttons = CreateUiObject("Buttons", panel.transform, typeof(VerticalLayoutGroup));
        var buttonsRect = (RectTransform)buttons.transform;
        buttonsRect.anchorMin = buttonsRect.anchorMax = buttonsRect.pivot = new Vector2(0.5f, 0.5f);
        buttonsRect.anchoredPosition = new Vector2(0f, -45f);
        buttonsRect.sizeDelta = new Vector2(360f, 260f);

        var layout = buttons.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 24f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateButton(buttons.transform, "ContinueButton", "Continue", OnContinueClicked);
        CreateButton(buttons.transform, "SettingsButton", "Settings", OnSettingsClicked);
    }

    // ---------- Logic ----------

    private void OnContinueClicked()
    {
        if (IsBusy) return;
        Close();   // PauseController restores time in its onClosed callback
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Pause: Settings clicked");
    }

    // ---------- Helpers ----------

    private static void CreateButton(Transform parent, string name, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = CreateUiObject(name, parent, typeof(Image), typeof(Button), typeof(LayoutElement));
        go.GetComponent<Image>().color = new Color(0.25f, 0.45f, 0.85f, 1f);
        go.GetComponent<LayoutElement>().preferredHeight = 90f;
        go.GetComponent<Button>().onClick.AddListener(onClick);

        GameObject text = CreateUiObject("Label", go.transform, typeof(TextMeshProUGUI));
        Stretch((RectTransform)text.transform);
        SetText(text.GetComponent<TextMeshProUGUI>(), label, 40f);
    }

    private static GameObject CreateUiObject(string name, Transform parent, params Type[] components)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        foreach (Type component in components) go.AddComponent(component);
        return go;
    }

    private static void SetText(TextMeshProUGUI text, string value, float size)
    {
        if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}