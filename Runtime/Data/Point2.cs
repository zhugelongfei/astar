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

        public static Point2 operator *(Point2 p, int val)
        {
            return new Point2(p.x * val, p.y * val);
        }

        public static bool operator ==(Point2 a, Point2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Point2 a, Point2 b)
        {
            return a.x != b.x || a.y != b.y;
        }
    }
}