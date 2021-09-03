using System.Collections.Generic;

namespace AStar
{
    public interface IAstarDebug
    {
        void Init();

        void SetPathNode(Node node);

        void SetCloseNodeList(ICollection<Node> nodeList);

        void Log(string str);
    }
}