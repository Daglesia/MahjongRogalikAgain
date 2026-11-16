using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Location : LocationColleague, ILocation
{
    [SerializeField] private LocationBase startingLocation;

    private LocationDefinition locationDefinition;

    public LocationDefinition Definition => locationDefinition;

    void Awake()
    {
        if (startingLocation != null) Initialize(startingLocation);
    }

    public Location Initialize(LocationBase locationBase)
    {
        locationDefinition = new LocationDefinition();
        locationDefinition.SetLocation(locationBase);
        locationDefinition.SetClicked(false);
        return this;
    }

    public LocationBase GetLocationBase() => locationDefinition.GetLocation();

    public LocationType GetLocationType() => locationDefinition.GetLocation().GetLocationType();
    public string GetTitle() => locationDefinition.GetLocation().GetTitle();

    public void Deselect() => locationDefinition.SetClicked(false);

    public void SetInteractable(bool value) => GetComponent<Collider2D>().enabled = value;

    void Start()
    {
        Mediator = FindAnyObjectByType<LocationMediator>();
    }

    void OnMouseDown()
    {
        if (locationDefinition.GetIsClicked())
        {
            Mediator?.NotifyLocationExit(this);
            return;
        }

        locationDefinition.SetClicked(true);
        Mediator?.NotifyLocationSelected(this);
    }
}
