namespace Lonfee.AStar
{
    public class Point2 : Lonfee.ObjectPool.IPoolObject
    {
        public int x;
        public int y;

        public int Key
        {
            get { return x + (y << 16); }
        }

        public Point2 InitData(int x, int y)
        {
            this.x = x;
            this.y = y;
            return this;
        }

        public void OnPop()
        {

        }

        public void OnPush()
        {
            x = 0;
            y = 0;
        }

        public void OnDestroy()
        {

        }

    }
}