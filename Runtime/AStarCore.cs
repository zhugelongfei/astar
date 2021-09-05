using System;
using System.Collections.Generic;
using Lonfee.ObjectPool;
using Mehroz;

namespace Lonfee.AStar
{
    public class AStarCore
    {
        public delegate bool CheckIsBlock(int x, int y);

        private const int SLANT_CONSUME = 14;
        private const int STRAIGHT_CONSUME = 10;

        private PriorityQueue<Node> openQueue = new PriorityQueue<Node>(new NodeComparator());
        private Dictionary<int, Node> openDic = new Dictionary<int, Node>();
        private Dictionary<int, Node> closeDic = new Dictionary<int, Node>();

        private ObjectPool<Node> nodePool = new ObjectPool_DefaultFactory<Node>();

        private CheckIsBlock checkIsBlockFunction = null;

        private IAstarDebug debugTools = null;

        private const int DIR_STRAIGHT_START_INDEX = 4;
        private const int DIR_SLANT_END_INDEX = 4;
        private const int DIR_LENGTH = 8;
        private int dirStartIndex = 0;
        private int[,] directionValue = new int[,]
        {
            // slant
            { -1, -1 },
            { -1, 1 },
            { 1, -1 },
            { 1, 1 },

            // straight
            { 0, -1 },
            { 0, 1 },
            { -1, 0 },
            { 1, 0 },
        };

        public AStarCore(CheckIsBlock checkIsBlockFunction, bool canSlantMove, IAstarDebug debugTools)
        {
            this.checkIsBlockFunction = checkIsBlockFunction;
            this.debugTools = debugTools;
            dirStartIndex = canSlantMove ? 0 : DIR_STRAIGHT_START_INDEX;

            if (checkIsBlockFunction == null)
                throw new NullReferenceException("Check point is block function is null.");
        }

        public List<Point2> FindPath(int startX, int startY, int endX, int endY)
        {
            // start and end must not be block
            if (checkIsBlockFunction(startX, startY) || checkIsBlockFunction(endX, endY))
                return null;

            // is already in end ?
            if (startX == endX && startY == endY)
                return null;

#if DEBUG_LONFEE_ASTAR
            if (debugTools != null)
                debugTools.Init();
#endif

            // push start point to open set
            Node startNode = nodePool.Pop();
            startNode.x = startX;
            startNode.y = startY;
            startNode.g = 0;
            startNode.h = (Math.Abs(endX - startX) + Math.Abs(endY - startY)) * STRAIGHT_CONSUME;
            startNode.f = startNode.g + startNode.h;
            AddToOpenDic(startNode);

            // start find path
            while (openQueue.Count > 0)
            {
                // pop an open point
                Node tempNode = PopAndOpenPoint();

                // is target ?
                if (tempNode.x == endX && tempNode.y == endY)
                {
#if DEBUG_LONFEE_ASTAR
                    if (debugTools != null)
                    {
                        debugTools.SetCloseNodeList(closeDic.Values);
                        debugTools.SetPathNode(tempNode);
                    }
#endif

                    List<Point2> result = GetTotalPoint(tempNode);

                    // clear
                    openDic.Clear();
                    openQueue.Clear();
                    closeDic.Clear();
                    nodePool.PushAllUsedObject();

                    return result;
                }

                // find round point
                for (int i = dirStartIndex; i < DIR_LENGTH; i++)
                {
                    // calculate point info
                    int x = tempNode.x + directionValue[i, 0];
                    int y = tempNode.y + directionValue[i, 1];
                    int key = Node.CalculateKey(x, y);

                    // check is block
                    if (checkIsBlockFunction(x, y))
                    {
                        AddToCloseDic(tempNode);
                        continue;
                    }

                    // check is in close
                    if (closeDic.ContainsKey(key))
                        continue;

                    // check can slant move (require two slant point can be pass)
                    bool isSlant = i < DIR_SLANT_END_INDEX;
                    if (checkIsBlockFunction(x, tempNode.y) || checkIsBlockFunction(tempNode.x, y))
                        continue;

                    int g = tempNode.g + (isSlant ? SLANT_CONSUME : STRAIGHT_CONSUME);
                    int h = (Math.Abs(endX - x) + Math.Abs(endY - y)) * STRAIGHT_CONSUME;
                    int f = g + h;

                    // push to open
                    if (openDic.ContainsKey(key))
                    {
                        // oh, it is already in open set, update info
                        Node inOpenNode = openDic[key];
                        if (inOpenNode.f > f)
                        {
                            inOpenNode.f = f;
                            inOpenNode.g = g;
                            inOpenNode.parent = tempNode;
                        }
                    }
                    else
                    {
                        // it is not in open set, push it
                        Node aroundNode = nodePool.Pop();
                        aroundNode.x = x;
                        aroundNode.y = y;
                        aroundNode.f = f;
                        aroundNode.g = g;
                        aroundNode.h = h;
                        aroundNode.parent = tempNode;
                        AddToOpenDic(aroundNode);
                    }
                }
            }

#if DEBUG_LONFEE_ASTAR
            if (debugTools != null)
                debugTools.SetCloseNodeList(closeDic.Values);
#endif

            return null;
        }

        private void AddToOpenDic(Node node)
        {
            openDic.Add(node.Key, node);
            openQueue.Push(node);
        }

        private Node PopAndOpenPoint()
        {
            Node tempNode = openQueue.Pop();
            openDic.Remove(tempNode.Key);
            AddToCloseDic(tempNode);

            return tempNode;
        }

        private void AddToCloseDic(Node node)
        {
            if (!closeDic.ContainsKey(node.Key))
            {
                closeDic.Add(node.Key, node);
            }
        }

        /// <summary>
        /// Get total point by last node
        /// </summary>
        private List<Point2> GetTotalPoint(Node lastNode)
        {
            if (lastNode == null)
                return null;

            int count = 0;
            Node temp = lastNode;
            while (temp != null)
            {
                count++;
                temp = temp.parent;
            }

            List<Point2> allNodeList = new List<Point2>(count);
            temp = lastNode;
            while (temp != null)
            {
                Point2 point = new Point2();
                point.x = temp.x;
                point.y = temp.y;
                allNodeList.Add(point);
                temp = temp.parent;
            }

            return allNodeList;
        }

        #region Folyd Moothness

        /// <summary>
        /// Folyd Moothness Algorithm
        /// </summary>
        /// <param name="node">last node</param>
        /// <returns>Point set</returns>
        public void FolydMoothnessPath(List<Point2> allPointList)
        {
            if (allPointList == null || allPointList.Count <= 2)
                return;

            // 1: filter straight point
            FilterStraightLine(allPointList);

            // 2: filter cross point waklable
            int len = allPointList.Count;
            for (int i = len - 1; i > 0; i--)
            {
                for (int j = 0; j <= i - 1; j++)
                {
                    if (CheckCrossPointWalkable(allPointList[i], allPointList[j]))
                    {
                        allPointList.RemoveRange(j + 1, i - j - 1);
                        //for (int k = i - 1; k > j; k--)
                        //{
                        //    allPointList.RemoveAt(k);
                        //}
                        i = j;
                        break;
                    }
                }
            }
        }

        private void FilterStraightLine(List<Point2> path)
        {
            if (path.Count <= 2)
                return;

            int len = path.Count;
            Point2 vector = path[len - 1] - path[len - 2];
            Point2 tempvector;
            for (int i = len - 3; i >= 0; i--)
            {
                tempvector = path[i + 1] - path[i];
                if ((vector.x == 0 && tempvector.x == 0) || (vector.y == 0 && tempvector.y == 0))
                {
                    path.RemoveAt(i + 1);
                }
                else
                {
                    vector = tempvector;
                }
            }
        }

        private bool CheckCrossPointWalkable(Point2 startPoint, Point2 endPoint)
        {
            if (startPoint == endPoint)
                return true;

            int minY = Math.Min(startPoint.y, endPoint.y);
            int maxY = Math.Max(startPoint.y, endPoint.y);

            int minX = Math.Min(startPoint.x, endPoint.x);
            int maxX = Math.Max(startPoint.x, endPoint.x);

            Dictionary<int, Point2> pointList = new Dictionary<int, Point2>();//交点集合

            // 1: add start and end
            AddPoint2(pointList, startPoint.x, startPoint.y);
            AddPoint2(pointList, endPoint.x, endPoint.y);

            //--2 add cross points
            if (endPoint.x == startPoint.x)
            {
                // x axis translation
                int x = endPoint.x;
                for (int y = minY; y <= maxY; y++)
                {
                    AddPoint2(pointList, x, y);
                }
            }
            else if (endPoint.y == startPoint.y)
            {
                // y axis translation
                int y = endPoint.y;
                for (int x = minX; x <= maxX; x++)
                {
                    AddPoint2(pointList, x, y);
                }
            }
            else
            {
                // slant
                //使用直线的斜率公式计算交点
                float x1 = startPoint.x + 0.5f;
                float y1 = startPoint.y + 0.5f;
                float x2 = endPoint.x + 0.5f;
                float y2 = endPoint.y + 0.5f;

                long dx = (long)(x2 * 10 - x1 * 10);
                long dy = (long)(y2 * 10 - y1 * 10);

                //1:求出直线公式y=kx+b
                Fraction K = new Fraction(dy, dx);
                Fraction K2 = new Fraction(dx, dy);
                Fraction B = new Fraction((long)(y1 * 10), 10) - K * new Fraction((long)(x1 * 10), 10);

                Fraction tempFrac = new Fraction();

                //2:先进行x方向递增，算出位置
                for (int x = minX; x <= maxX; x++)
                {
                    float folydY = ((K * (tempFrac.InitData(x)) + B).ToFloat());
                    int y = (int)folydY;

                    if (y >= minY && y <= maxY)
                    {
                        // on the line
                        AddPoint2(pointList, x, y);

                        if (y == folydY)
                        {
                            //证明：刚好是顶点
                            AddAroundPoint(pointList, x, y, minX, minY);
                        }
                        else
                        {
                            //不是顶点，检测左边的格子
                            if (x - 1 >= minX)
                            {
                                //证明：与格子的x边的交点（X增长，所以是与X边交点），也要检测这个点左边的格子
                                AddPoint2(pointList, x - 1, y);
                            }
                        }
                    }
                }

                //3:再进行y方向递增，算出位置
                for (int y = minY; y <= maxY; y++)
                {
                    float folydX = ((tempFrac.InitData(y) - B) * K2).ToFloat();
                    int x = (int)folydX;

                    if (x >= minX && x <= maxX)
                    {
                        //证明：不在线段的延长线上
                        AddPoint2(pointList, x, y);

                        if (x == folydX)
                        {
                            //证明：刚好是顶点
                            AddAroundPoint(pointList, x, y, minX, minY);
                        }
                        else
                        {
                            //不是顶点，检测下边的格子
                            if (y - 1 >= minY)
                            {
                                //证明：与格子的y边的交点（y增长，所以是与y边交点），也要检测这个点下边的格子
                                AddPoint2(pointList, x, y - 1);
                            }
                        }
                    }
                }
            }

            //--3:逐个扫描每个点
            foreach (Point2 point in pointList.Values)
            {
                if (checkIsBlockFunction(point.x, point.y))
                    return false;
            }

            return true;
        }

        private void AddAroundPoint(Dictionary<int, Point2> pointList, int x, int y, int minx, int miny)
        {
            if (x - 1 >= minx)
                AddPoint2(pointList, x - 1, y);

            if (y - 1 >= miny)
                AddPoint2(pointList, x, y - 1);

            if (x - 1 >= minx && y - 1 >= miny)
                AddPoint2(pointList, x - 1, y - 1);
        }

        private void AddPoint2(Dictionary<int, Point2> pointList, int x, int y)
        {
            Point2 p = new Point2(x, y);
            if (!pointList.ContainsKey(p.Key))
            {
                pointList.Add(p.Key, p);
            }
        }

#endregion


    }
}