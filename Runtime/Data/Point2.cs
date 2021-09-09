namespace Lonfee.AStar
{
    public struct Point2
    {
        public int x;
        public int y;

        public int Key
        {
            get { return x + (y << 16); }
        }

        public Point2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Point2 operator -(Point2 a, Point2 b)
        {
            return new Point2(a.x - b.x, a.y - b.y);
        }

        public static Point2 operator +(Point2 a, Point2 b)
        {
            return new Point2(a.x + b.x, a.y + b.y);
        }

        public static bool operator ==(Point2 a, Point2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Point2 a, Point2 b)
        {
            return a.x != b.x || a.y != b.y;
        }

        public override bool Equals(object obj)
        {
            return obj is Point2 point && x == point.x && y == point.y;
        }

        public override int GetHashCode()
        {
            int hashCode = -464793961;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return string.Format("Point2({0}, {1})", x, y);
        }

    }
}