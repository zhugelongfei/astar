using System.Collections.Generic;

namespace AStar
{
    internal class NodeComparator : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            return x.f.CompareTo(y.f);
        }
    }
}