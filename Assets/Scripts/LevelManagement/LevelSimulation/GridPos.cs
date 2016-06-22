namespace LevelSimulation
{
    public class GridPos
    {
        public int r;
        public int c;

        public GridPos(int r, int c)
        {
            this.r = r;
            this.c = c;
        }

        public GridPos(GridPos other)
        {
            r = other.r;
            c = other.c;
        }

        public static bool operator ==(GridPos a, GridPos b)
        {
            return a.r == b.r && a.c == b.c;
        }

        public static bool operator !=(GridPos a, GridPos b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is GridPos))
            {
                return false;
            }

            GridPos other = (GridPos)obj;
            return r == other.r && c == other.c;
        }

        public override int GetHashCode()
        {
            return (r << 16) + c;
        }
    }
}