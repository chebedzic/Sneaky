namespace Features.Enemy.Domain
{
    public readonly struct GridCoord
    {
        public readonly int X;
        public readonly int Y;

        public GridCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj) => obj is GridCoord other && X == other.X && Y == other.Y;

        public override int GetHashCode() => (X, Y).GetHashCode();

        public static bool operator ==(GridCoord a, GridCoord b) => a.Equals(b);

        public static bool operator !=(GridCoord a, GridCoord b) => !a.Equals(b);
    }
}
