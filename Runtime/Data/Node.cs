using System;

namespace Lonfee.AStar
{
    internal class Node : ObjectPool.IPoolObject, IComparable<Node>
    {
        public int x;
        public int y;

        public int g;
        public int h;
        public int f;

        public Node parent;

        public int Key
        {
            get
            {
                return CalculateKey(x, y);
            }
        }

        public static int CalculateKey(int x, int y)
        {
            return (x << 16) + y;
        }

        public void Reset()
        {
            x = 0;
            y = 0;
            g = 0;
            h = 0;
            f = 0;
            parent = null;
        }

        public void OnPush()
        {
            Reset();
        }

        public void OnPop()
        {

        }

        public void OnDestroy()
        {

        }

        public int CompareTo(Node other)
        {
            return this.f.CompareTo(other.f);
        }

        public override string ToString()
        {
            return string.Format("Node(x:{0}, y:{1}, g:{2}, h:{3}, f:{4})", x, y, g, h, f);
        }
    }
}