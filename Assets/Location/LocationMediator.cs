using UnityEngine;

public class LocationMediator : MonoBehaviour, ILocationMediator
{
    [Tooltip("Optional. Found automatically in the scene if left empty.")]
    [SerializeField] private ViewManager viewManager;

    private PlayerMarker player;
    private bool isMoving;
    private bool inEvent;   // a location view is open or transitioning

    public void RegisterPlayer(PlayerMarker marker) => player = marker;

    public void NotifyLocationSelected(Location location)
    {
        // Location flags itself as clicked; a map node is never a "selected" toggle
        location.Deselect();

        if (isMoving || inEvent || player == null) return;

        RunState state = RunManager.Instance.State;
        if (state?.Map == null) return;

        // Only nodes directly connected to the current one are valid
        if (!state.CurrentNode.NextIds.Contains(location.NodeId)) return;

        isMoving = true;
        state.CurrentNodeId = location.NodeId;   // position is tracked in the run state

        player.MoveTo(state.Map.Nodes[location.NodeId].Position, () =>
        {
            isMoving = false;
            NotifyLocationEnter(location);
        });
    }

    public void NotifyLocationEnter(Location location)
    {
        LocationType type = location.GetLocationType();
        Debug.Log($"Entered {type} (node {location.NodeId})");

        switch (type)
        {
            case LocationType.Combat:
            case LocationType.BossCombat:
                EnterCombat(type);
                break;

            case LocationType.EquivalentExchange:
                OpenView<EquivalentExchangeView>();
                break;

            case LocationType.Shop:
                OpenView<ShopView>();
                break;

            // todo: Stone, CardFreebie, BlackHole -> OpenView<...View>() etc.
            default:
                break;
        }
    }

    private void EnterCombat(LocationType type)
    {
        RunState state = RunManager.Instance.State;
        var context = CombatContext.FromRun(state, type);
        SceneLoader.Instance.StartCombat(context);
    }

    private void OpenView<T>() where T : ViewBase
    {
        if (viewManager == null) viewManager = FindAnyObjectByType<ViewManager>();
        if (viewManager == null)
        {
            Debug.LogError("LocationMediator: no ViewManager in the scene.", this);
            return;
        }

        T view = viewManager.Get<T>();
        if (view == null) return;

        inEvent = true;   // map nodes use OnMouseDown, which UI does not block
        view.Open(RunManager.Instance.State, () => inEvent = false);
    }

    public void NotifyLocationExit(Location location)
    {
        location.Deselect();
    }
}