public class ItemContext
{
    public DeckManager Deck { get; }
    public DiscardManager Discards { get; }
    public HandManager Hand { get; }

    public ItemContext(DeckManager deck, DiscardManager discards, HandManager hand)
    {
        Deck = deck;
        Discards = discards;
        Hand = hand;
    }
}