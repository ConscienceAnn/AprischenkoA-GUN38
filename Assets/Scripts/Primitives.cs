 public enum NeighbourType
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,

        LeftRight = Left | Right,
        TopBottom = Top | Bottom,
        All = Left | Right | Top | Bottom
    }

    public enum Team
    {
        Player1,
        Player2
    }

