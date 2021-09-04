namespace Lonfee.AStar
{
    public class Node : Lonfee.ObjectPool.IPoolObject
    {
        public int x;
        public int y;

        /// <summary>
        /// 从初始点到N点的实际消耗
        /// </summary>
        public int g;

        /// <summary>
        /// 从N点到终点的预估消耗
        /// </summary>
        public int h;

        /// <summary>
        /// 从初始点经过N点到终点的预估消耗 F = G + H
        /// </summary>
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
    }
}