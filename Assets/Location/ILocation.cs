    public enum LocationType
{
    Start, Combat, BossCombat, EquivalentExchange, Shop, Stone, CardFreebie, BlackHole
}
    
    public interface ILocation
    {
        LocationType GetLocationType();
        string GetTitle();
    }