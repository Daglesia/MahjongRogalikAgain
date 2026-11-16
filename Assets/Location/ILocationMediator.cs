using System.Collections.Generic;
using UnityEngine;

public interface ILocationMediator
{
    void NotifyLocationSelected(Location location);
    void NotifyLocationEnter(Location location);
    void NotifyLocationExit(Location location);
}
