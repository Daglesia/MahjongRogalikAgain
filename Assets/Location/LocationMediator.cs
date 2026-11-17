using UnityEngine;

public class LocationMediator : MonoBehaviour, ILocationMediator
{
    private PlayerMarker player;
    private bool isMoving;

    public void RegisterPlayer(PlayerMarker marker) => player = marker;

    public void NotifyLocationSelected(Location location)
    {
        // Location flags itself as clicked; a map node is never a "selected" toggle
        location.Deselect();

        if (isMoving || player == null) return;

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

            // todo: Shop, EquivalentExchange, Stone, CardFreebie, BlackHole
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

    public void NotifyLocationExit(Location location)
    {
        location.Deselect();
    }
}