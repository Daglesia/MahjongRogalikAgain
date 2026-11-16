using UnityEngine;

public class LocationDefinition
{
    private LocationBase location;
    private bool isClicked;

    public void SetLocation(LocationBase newLocation) => location = newLocation;

    public Sprite GetLocationSprite() => location != null ? location.art : null;
    public LocationBase GetLocation() => location;
    public bool GetIsClicked() => isClicked;
    public void SetClicked(bool flag) => isClicked = flag;
}