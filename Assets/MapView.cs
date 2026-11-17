using System.Collections.Generic;
using UnityEngine;

public class MapView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MapGenerator generator;
    [Tooltip("Assign all 8 location assets from Assets/Location/Locations")]
    [SerializeField] private LocationBase[] catalog;
    [Tooltip("Assets/Shaders/MapLine.mat (shader: Universal Render Pipeline/2D/Sprite-Unlit-Default)")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Camera cam;

    [Header("Look")]
    [SerializeField] private float nodeWidth = 0.8f;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private float cameraMargin = 1f;

    private Transform nodesRoot;
    private Transform edgesRoot;
    private readonly Dictionary<LocationType, LocationBase> byType = new Dictionary<LocationType, LocationBase>();

    void Start()
    {
        if (RunManager.Instance.State == null) RunManager.Instance.StartNewRun();
        int seed = RunManager.Instance.State.Seed;

        foreach (var asset in catalog)
            if (asset != null) byType[asset.GetLocationType()] = asset;

        MapData map = generator.GenerateMap(seed);
        Build(map);
        FitCamera(map);
    }

    private void Build(MapData map)
    {
        nodesRoot = new GameObject("Nodes").transform;
        nodesRoot.SetParent(transform, false);
        edgesRoot = new GameObject("Edges").transform;
        edgesRoot.SetParent(transform, false);

        float radius = nodeWidth * 0.5f;

        foreach (MapNode node in map.Nodes)
            CreateNode(node);

        foreach (MapNode node in map.Nodes)
        {
            foreach (int nextId in node.NextIds)
                CreateLine(node.Position, map.Nodes[nextId].Position, radius);
        }
    }

    private void CreateNode(MapNode node)
    {
        if (!byType.TryGetValue(node.Type, out LocationBase asset))
        {
            Debug.LogError($"MapView: no catalog asset for {node.Type}");
            return;
        }

        var go = new GameObject($"Node_{node.Id}_{node.Type}");
        go.transform.SetParent(nodesRoot, false);
        go.transform.position = node.Position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        sr.sprite = asset.art;

        // Scale so the art is ~nodeWidth wide
        if (sr.sprite != null)
        {
            float s = nodeWidth / sr.sprite.bounds.size.x;
            go.transform.localScale = new Vector3(s, s, 1f);
        }

        // Collider before Location (Location requires a Collider2D)
        go.AddComponent<BoxCollider2D>();
        var location = go.AddComponent<Location>();
        location.Initialize(asset);

        // Location doesn't set its own sprite
        sr.sprite = location.Definition.GetLocationSprite();
    }

    private void CreateLine(Vector2 a, Vector2 b, float radius)
    {
        Vector2 dir = (b - a).normalized;
        Vector2 from = a + dir * radius;
        Vector2 to = b - dir * radius;

        var go = new GameObject("Edge");
        go.transform.SetParent(edgesRoot, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.sortingOrder = 0;
        lr.numCapVertices = 0;
        lr.startColor = Color.white;
        lr.endColor = Color.white;
        lr.material = GetLineMaterial();
    }

    private Material GetLineMaterial()
    {
        if (lineMaterial == null)
            Debug.LogWarning("MapView: assign MapLine.mat to Line Material (lines will be pink/invisible otherwise).");
        return lineMaterial;
    }

    private void FitCamera(MapData map)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null || map.Nodes.Count == 0) return;

        var bounds = new Bounds(map.Nodes[0].Position, Vector3.zero);
        foreach (MapNode n in map.Nodes)
            bounds.Encapsulate(n.Position);
        bounds.Expand(cameraMargin * 2f);

        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max(bounds.size.y / 2f, bounds.size.x / 2f / cam.aspect);
        cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
    }
}