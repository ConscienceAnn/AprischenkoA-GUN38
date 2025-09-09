

namespace Game.Casino.GameEntities
{
    public readonly struct Card
    {
        public Suit Suit { get; }
        public Rank Rank { get; }

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }
        public override string ToString() => $"{Rank} of {Suit}";
    }
}

