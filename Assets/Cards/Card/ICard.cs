    public enum CardValue
{
    Bamboo1, Bamboo2, Bamboo3, Bamboo4, Bamboo5, Bamboo6,
    Earth, Fire, Water, Wind
}
    
    public interface ICard
    {
        CardValue[] GetCardValues();
        string GetDescription();
    }