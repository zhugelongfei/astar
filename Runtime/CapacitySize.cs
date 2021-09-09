namespace Lonfee.AStar
{
    public struct CapacitySize
    {
        public int openSetSize;
        public int closeSetSize;
        public int poolInitSize;
        public int poolMaxSize;

        public static CapacitySize Default
        {
            get
            {
                CapacitySize data = new CapacitySize();
                data.openSetSize = 32;
                data.closeSetSize = 32;
                data.poolInitSize = 16;
                data.poolMaxSize = int.MaxValue;

                return data;
            }
        }
    }
}