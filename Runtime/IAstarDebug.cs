using System.Collections.Generic;

namespace Lonfee.AStar
{
    public interface IAstarDebug
    {
        void SetPath(ICollection<Point2> nodeList);

        void SetClosePointCollection(ICollection<Point2> nodeList);

        void SetOpenPointCollection(ICollection<Point2> nodeList);
    }
}