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
        Debug.Log($"Entered {location.GetLocationType()} (node {location.NodeId})");
        // todo: load combat / shop / etc. based on location.GetLocationType()
    }

    public void NotifyLocationExit(Location location)
    {
        location.Deselect();
    }
}