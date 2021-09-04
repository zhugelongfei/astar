using System.Collections.Generic;

namespace Lonfee.AStar
{
    internal class NodeComparator : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            return x.f.CompareTo(y.f);
        }
    }
}