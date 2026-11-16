using UnityEngine;

public class LocationMediator : MonoBehaviour, ILocationMediator
{
    public void NotifyLocationSelected(Location location)
    {
    }

    public void NotifyLocationEnter(Location location)
    {
    }

    public void NotifyLocationExit(Location location)
    {
        location.Deselect();
    }
}
