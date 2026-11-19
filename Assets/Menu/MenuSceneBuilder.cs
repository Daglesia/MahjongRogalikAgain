using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// Tools > Create Menu Scene
/// Builds Assets/Scenes/MenuScene.unity and puts it first in Build Settings.
public static class MenuSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/MenuScene.unity";
    private const string DeckPath = "Assets/Cards/Decks/BasicDeck.asset";
    private const string NebulaMatPath = "Assets/Shaders/NebulaBackground.mat";
    private const string SquareSpriteGuid = "311925a002f4447b3a28927169b83ea6"; // same sprite CombatScene uses

    [MenuItem("Tools/Create Menu Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateBackground();
        CreateRunManager();
        new GameObject("MenuMediator").AddComponent<MenuMediator>();
        CreateUi();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings();

        Debug.Log($"Menu scene created at {ScenePath} and set as the first scene in Build Settings.");
    }

    // ---------- Scene objects ----------

    private static void CreateCamera()
    {
        var go = new GameObject("Main Camera") { tag = "MainCamera" };
        go.transform.position = new Vector3(0f, 0f, -10f);

        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.012f, 0.02f, 0.047f, 1f);

        go.AddComponent<AudioListener>();
        go.AddComponent<UniversalAdditionalCameraData>();
    }

    /// Optional nebula backdrop; silently skipped if the assets can't be found.
    private static void CreateBackground()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(SquareSpriteGuid));
        var material = AssetDatabase.LoadAssetAtPath<Material>(NebulaMatPath);
        if (sprite == null || material == null) return;

        var go = new GameObject("Background");
        go.transform.localScale = new Vector3(30f, 18f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sharedMaterial = material;
        sr.sortingOrder = -10;
    }

    /// The Singleton must exist here WITH its starting deck, otherwise it would be created
    /// empty on first access and MapScene's own RunManager would destroy itself as a duplicate.
    private static void CreateRunManager()
    {
        var run = new GameObject("RunManager").AddComponent<RunManager>();
        var deck = AssetDatabase.LoadAssetAtPath<DeckGeneration>(DeckPath);

        if (deck == null)
        {
            Debug.LogWarning($"MenuSceneBuilder: couldn't find {DeckPath}. Assign 'Starting Deck' on RunManager manually.");
            return;
        }

        var so = new SerializedObject(run);
        so.FindProperty("startingDeck").objectReferenceValue = deck;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateUi()
    {
        var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Title
        var title = CreateText("Title", canvasGo.transform, "Mahjong Rogalik", 96f);
        var titleRect = (RectTransform)title.transform;
        titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -140f);
        titleRect.sizeDelta = new Vector2(1200f, 130f);

        // Button column
        var panel = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        panel.transform.SetParent(canvasGo.transform, false);

        var panelRect = (RectTransform)panel.transform;
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, -80f);
        panelRect.sizeDelta = new Vector2(420f, 360f);

        var layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 24f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateButton(panel.transform, "NewRunButton", "New Run", MenuButton.Kind.NewRun);
        CreateButton(panel.transform, "SettingsButton", "Settings", MenuButton.Kind.Settings);
        CreateButton(panel.transform, "ExitButton", "Exit", MenuButton.Kind.Exit);
    }

    private static void CreateButton(Transform parent, string name, string label, MenuButton.Kind kind)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        go.GetComponent<Image>().color = new Color(0.25f, 0.45f, 0.85f, 1f);
        go.GetComponent<LayoutElement>().preferredHeight = 90f;

        var text = CreateText("Label", go.transform, label, 40f);
        var rect = (RectTransform)text.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var menuButton = go.AddComponent<MenuButton>();
        var so = new SerializedObject(menuButton);
        so.FindProperty("kind").enumValueIndex = (int)kind;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreateText(string name, Transform parent, string value, float size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = value;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return go;
    }

    private static void AddToBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        scenes.RemoveAll(s => s.path == ScenePath);
        scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}