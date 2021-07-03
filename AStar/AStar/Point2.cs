namespace AStar
{
    public class Point2 : Pool.IPoolItem
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

        public void DestroyObject()
        {

        }

        public void PopCallBack()
        {

        }

        public void PushCallBack()
        {
            x = 0;
            y = 0;
        }
    }
}