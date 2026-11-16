using UnityEngine;

[CreateAssetMenu(fileName = "NewLocation", menuName = "Locations/Basic Location")]
public class BasicLocation : LocationBase
{
    [SerializeField] private LocationType locationType;
    [SerializeField] private string title;

    public override LocationType GetLocationType() => locationType;
    public override string GetTitle() => title;
    public Sprite GetSprite() => art;
}
