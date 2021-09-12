using Mehroz;
using System;
using Lonfee.ObjectPool;
using System.Collections.Generic;

namespace Lonfee.AStar
{
    public class AStarCore
    {
        public delegate bool CheckIsBlock(int x, int y);

        private const int SLANT_CONSUME = 14;
        private const int STRAIGHT_CONSUME = 10;
        private const int DIR_STRAIGHT_START_INDEX = 4;
        private const int DIR_SLANT_END_INDEX = 4;
        private const int DIR_LENGTH = 8;

        private PriorityQueue<Node> openQueue;
        private Dictionary<int, Node> openDic;
        private Dictionary<int, Node> closeDic;
        private ObjectPool<Node> nodePool;

        private CheckIsBlock checkIsBlockFunction = null;

        private IAstarDebug debugTools = null;

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

        public AStarCore(CheckIsBlock checkIsBlockFunction, CapacitySize sizeInfo, bool canSlantMove = false, IAstarDebug debugTools = null)
        {
            if (checkIsBlockFunction == null)
                throw new NullReferenceException("Check block function can not be null.");

            this.checkIsBlockFunction = checkIsBlockFunction;
            this.dirStartIndex = canSlantMove ? 0 : DIR_STRAIGHT_START_INDEX;
            this.debugTools = debugTools;

            nodePool = new ObjectPool_DefaultFactory<Node>(sizeInfo.poolInitSize, 1, sizeInfo.poolMaxSize);
            openQueue = new PriorityQueue<Node>(sizeInfo.openSetSize);
            openDic = new Dictionary<int, Node>(sizeInfo.openSetSize);
            closeDic = new Dictionary<int, Node>(sizeInfo.closeSetSize);
        }

        public List<Point2> FindPath(int startX, int startY, int endX, int endY)
        {
            // start and end must not be block
            if (checkIsBlockFunction(startX, startY) || checkIsBlockFunction(endX, endY))
                return null;

            // is already in end ?
            if (startX == endX && startY == endY)
                return null;

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
                    List<Point2> result = GetTotalPoint(tempNode);

                    if (debugTools != null)
                    {
                        SetCloseToDebug();
                        SetOpenToDebug();
                        SetPathToDebug(result);
                    }

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

            if (debugTools != null)
            {
                SetCloseToDebug();
            }

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

            allNodeList.Reverse();
            return allNodeList;
        }

        #region Folyd Moothness

        /// <summary>
        /// Folyd Moothness Algorithm
        /// </summary>
        public void FolydMoothnessPath(List<Point2> allPointList, float unitRadius = 0.2f)
        {
            if (allPointList == null || allPointList.Count <= 2)
                return;

            // 1: filter straight point
            FilterStraightLine(allPointList);

            // 2: filter cross point waklable
            // Note: adjacent point is always can walkable
            for (int i = allPointList.Count - 1; i >= 2; i--)
            {
                for (int j = i - 2; j >= 0; j--)
                {
                    if (CheckCrossPointWalkable(allPointList[i], allPointList[j], unitRadius))
                    {
                        allPointList.RemoveRange(j + 1, 1);
                        i--;
                    }
                    else
                    {
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
            Point2 curDir = path[len - 1] - path[len - 2];
            for (int i = len - 3; i >= 0; i--)
            {
                Point2 nextDir = path[i + 1] - path[i];

                if (curDir == nextDir)
                {
                    path.RemoveAt(i + 1);
                }
                else
                {
                    curDir = nextDir;
                }
            }
        }

        private bool CheckCrossPointWalkable(Point2 startPoint, Point2 endPoint, float unitRadius)
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

            // 2: add cross points
            if (endPoint.x == startPoint.x)
            {
                // x axis translation
                int x = endPoint.x;
                for (int y = minY + 1; y < maxY; y++)
                {
                    AddPoint2(pointList, x, y);
                }
            }
            else if (endPoint.y == startPoint.y)
            {
                // y axis translation
                int y = endPoint.y;
                for (int x = minX + 1; x < maxX; x++)
                {
                    AddPoint2(pointList, x, y);
                }
            }
            else
            {
                // slant
                float x1 = startPoint.x + 0.5f;
                float y1 = startPoint.y + 0.5f;
                float x2 = endPoint.x + 0.5f;
                float y2 = endPoint.y + 0.5f;

                long dx = (long)(x2 * 10 - x1 * 10);
                long dy = (long)(y2 * 10 - y1 * 10);

                // 1: y = kx + b
                Fraction K = new Fraction(dy, dx);
                Fraction K2 = new Fraction(dx, dy);
                Fraction B = new Fraction((long)(y1 * 10), 10) - K * new Fraction((long)(x1 * 10), 10);
                Fraction B2 = new Fraction((long)(x1 * 10), 10) - K2 * new Fraction((long)(y1 * 10), 10);

                Fraction tempFrac = new Fraction();

                // 2: translation x
                for (int x = minX; x <= maxX; x++)
                {
                    float folydY = (K * tempFrac.InitData(x) + B).ToFloat();
                    int y = (int)folydY;

                    if (y >= minY && y <= maxY)
                    {
                        // on the line
                        int vertexY = (int)Math.Round(folydY);
                        float disSquare = DisWithPointToLine(K, B, x, vertexY);
                        if (disSquare > unitRadius * unitRadius)
                        {
                            AddPoint2(pointList, x, y);

                            // allright, point(x, y) is on x asix's whole num, so you must cross the left node
                            if (x - 1 >= minX)
                                AddPoint2(pointList, x - 1, y);
                        }
                        else
                        {
                            // the unit is so fat...
                            AddAroundPoint(pointList, x, vertexY, minX, minY);
                        }
                    }
                }

                // 3: translation y
                for (int y = minY; y <= maxY; y++)
                {
                    float folydX = ((tempFrac.InitData(y) - B) * K2).ToFloat();
                    int x = (int)folydX;

                    if (x >= minX && x <= maxX)
                    {
                        // on the line
                        int vertexX = (int)Math.Round(folydX);
                        float disSquare = DisWithPointToLine(K, B, vertexX, y);

                        if (disSquare > unitRadius * unitRadius)
                        {
                            AddPoint2(pointList, x, y);

                            if (y - 1 >= minY)
                                AddPoint2(pointList, x, y - 1);
                        }
                        else
                        {
                            AddAroundPoint(pointList, vertexX, y, minX, minY);
                        }
                    }
                }
            }

            // 3: check cross point is block
            foreach (Point2 point in pointList.Values)
            {
                if (checkIsBlockFunction(point.x, point.y))
                    return false;
            }

            return true;
        }

        private float DisWithPointToLine(Fraction k, Fraction b, Fraction xPos, Fraction yPos)
        {
            Fraction disNum = (k * xPos - yPos + b).Abs();

            Fraction dis = disNum * disNum / (k * k + 1);

            return dis.ToFloat();
        }

        private void AddAroundPoint(Dictionary<int, Point2> pointList, int x, int y, int minx, int miny)
        {
            if (x - 1 >= minx)
                AddPoint2(pointList, x - 1, y);

            if (y - 1 >= miny)
                AddPoint2(pointList, x, y - 1);

            if (x - 1 >= minx && y - 1 >= miny)
                AddPoint2(pointList, x - 1, y - 1);

            AddPoint2(pointList, x, y);
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

        #region Debug

        public void SetCloseToDebug()
        {
            if (debugTools == null)
                return;

            List<Point2> closeList = new List<Point2>(closeDic.Values.Count);
            foreach (var item in closeDic.Values)
            {
                closeList.Add(new Point2(item.x, item.y));
            }

            debugTools.SetClosePointCollection(closeList);
        }

        public void SetOpenToDebug()
        {
            if (debugTools == null)
                return;

            List<Point2> openList = new List<Point2>(openDic.Values.Count);
            foreach (var item in openDic.Values)
            {
                openList.Add(new Point2(item.x, item.y));
            }

            debugTools.SetOpenPointCollection(openList);
        }

        public void SetPathToDebug(List<Point2> pathList)
        {
            if (debugTools == null)
                return;

            List<Point2> result = new List<Point2>(pathList);

            debugTools.SetPath(result);
        }

        #endregion

    }
}