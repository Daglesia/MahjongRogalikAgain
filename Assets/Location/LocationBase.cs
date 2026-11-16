using UnityEngine;

public abstract class LocationBase : ScriptableObject, ILocation
{
    [SerializeField] public Sprite art;

    public abstract LocationType GetLocationType();
    public abstract string GetTitle();
}
