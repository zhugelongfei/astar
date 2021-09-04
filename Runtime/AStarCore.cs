using System;
using System.Collections.Generic;
using Lonfee.ObjectPool;

namespace Lonfee.AStar
{
    public class AStarCore
    {
        public delegate bool CheckIsBlock(float x, float y);

        private const int SLANT_CONSUME = 14;
        private const int STRAIGHT_CONSUME = 10;

        private PriorityQueue<Node> openQueue = new PriorityQueue<Node>(new NodeComparator());
        private Dictionary<int, Node> openDic = new Dictionary<int, Node>();
        private Dictionary<int, Node> closeDic = new Dictionary<int, Node>();

        private ObjectPool<Node> nodePool = new ObjectPool_DefaultFactory<Node>(int.MaxValue);

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

    }
}