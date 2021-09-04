using System;
using System.Collections.Generic;
using Mehroz;
using Lonfee.ObjectPool;

namespace Lonfee.AStar
{
    public class AStarCore
    {
        public delegate bool CheckIsBlock(float x, float y);

        private const int SLANT_CONSUME = 14;
        private const int LINE_CONSUME = 10;
        private const int NUM_PER_SESS = 20;

        private PriorityQueue<Node> openQueue = new PriorityQueue<Node>(new NodeComparator());
        private Dictionary<int, Node> openDic = new Dictionary<int, Node>();
        private Dictionary<int, Node> closeDic = new Dictionary<int, Node>();

        //对象池
        private ObjectPool<Node> nodePool = new ObjectPool_DefaultFactory<Node>(int.MaxValue);
        private ObjectPool<Point2> pointPool = new ObjectPool_DefaultFactory<Point2>(int.MaxValue);

        //检测是否是障碍点的委托
        private CheckIsBlock checkIsBlockFunction = null;

        //Debug工具
        private IAstarDebug debugTools = null;

        //方向
        private int[,] directionValue = new int[,] { { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 }, { 0, -1 }, { 0, 1 }, { -1, 0 }, { 1, 0 }, };
        private int dirStartIndex = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="checkIsBlockFunction">检测是否可通过的委托</param>
        /// <param name="canSlantMove">是否可以斜向移动</param>
        /// <param name="debugTools">调试对象</param>
        public AStarCore(CheckIsBlock checkIsBlockFunction, bool canSlantMove, IAstarDebug debugTools)
        {
            this.checkIsBlockFunction = checkIsBlockFunction;
            this.debugTools = debugTools;
            dirStartIndex = canSlantMove ? 0 : 4;

            if (checkIsBlockFunction == null)
                throw new NullReferenceException("Check point is block function is null.");
        }

        /// <summary>
        /// 寻路函数
        /// </summary>
        /// <param name="lastNode">终点</param>
        /// <returns>是否寻找到路径</returns>
        public bool FindPath(int startX, int startY, int endX, int endY, out Node lastNode)
        {
            #region 条件检测

            lastNode = null;

            //检测起始点和终点是否是障碍点，如果是则寻路失败
            if (checkIsBlockFunction(startX, startY) || checkIsBlockFunction(endX, endY))
                return false;

            //检测终点是否是起始点，如果是，则不需要寻路
            if (startX == endX && startY == endY)
                return false;

            #endregion

            //数据准备
            openDic.Clear();
            openQueue.Clear();
            closeDic.Clear();

            nodePool.PushAllUsedObject();

            if (debugTools != null)
                debugTools.Init();

            //将起始点放入Open集合
            Node startNode = nodePool.Pop();
            startNode.x = startX;
            startNode.y = startY;
            startNode.g = 0;
            startNode.h = (Math.Abs(endX - startX) + Math.Abs(endY - startY)) * LINE_CONSUME;
            startNode.f = startNode.g + startNode.h;
            AddToOpenDic(startNode);

            //开始寻路
            while (openQueue.Count > 0)
            {
                //从Open表中取出，放入Close表中
                Node tempNode = openQueue.Pop();
                openDic.Remove(tempNode.Key);
                AddToCloseDic(tempNode);

                //判断是否到达
                if (tempNode.x == endX && tempNode.y == endY)
                {
                    if (debugTools != null)
                    {
                        debugTools.SetCloseNodeList(closeDic.Values);
                        debugTools.SetPathNode(tempNode);
                    }
                    lastNode = tempNode;
                    return true;
                }

                //遍历周围的点
                for (int i = dirStartIndex; i < directionValue.GetLength(0); i++)
                {
                    //生成这个点
                    int x = tempNode.x + directionValue[i, 0];
                    int y = tempNode.y + directionValue[i, 1];
                    int key = Node.CalculateKey(x, y);

                    //检测此点是否是障碍点
                    if (checkIsBlockFunction(x, y))
                    {
                        //在这里，如果这个点超出地图区域，那么这个点也会放入Close表中
                        //不过无妨，因为此点不会被使用
                        AddToCloseDic(tempNode);
                        continue;
                    }

                    //检测斜向是否可通过（斜着经过的两个点都不为障碍点，则可斜向通行）
                    bool isSlant = Math.Abs(directionValue[i, 0]) + Math.Abs(directionValue[i, 1]) > 1;
                    if (checkIsBlockFunction(x, tempNode.y) || checkIsBlockFunction(tempNode.x, y))
                    {
                        continue;
                    }

                    //检测此点是否在Close表中
                    if (closeDic.ContainsKey(key))
                    {
                        continue;
                    }

                    int g = tempNode.g + (isSlant ? SLANT_CONSUME : LINE_CONSUME);
                    int h = (Math.Abs(endX - x) + Math.Abs(endY - y)) * LINE_CONSUME;
                    int f = g + h;

                    //检测此点是否在Open表中
                    if (openDic.ContainsKey(key))
                    {
                        Node inOpenNode = openDic[key];
                        if (inOpenNode.f > f)
                        {
                            inOpenNode.f = f;
                            inOpenNode.g = g;
                            inOpenNode.parent = tempNode;
                        }
                        continue;
                    }

                    //把这个点当成一个新的点，加入到Open表中
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

            if (debugTools != null)
                debugTools.SetCloseNodeList(closeDic.Values);

            return false;
        }

        /// <summary>
        /// 将一个点添加到开启列表
        /// </summary>
        private void AddToOpenDic(Node node)
        {
            openDic.Add(node.Key, node);
            openQueue.Push(node);
        }

        /// <summary>
        /// 将一个点添加到关闭列表
        /// </summary>
        private void AddToCloseDic(Node node)
        {
            if (!closeDic.ContainsKey(node.Key))
            {
                closeDic.Add(node.Key, node);
            }
        }

        /// <summary>
        /// 弗洛伊德平滑算法
        /// </summary>
        /// <param name="node">终点</param>
        /// <returns>拐点集合</returns>
        public List<Point2> FolydmoothnessPath(Node node)
        {
            List<Point2> allNodeList = new List<Point2>(GetAllNodeCount(node));

            while (node != null)
            {
                Point2 point = new Point2();
                point.x = node.x;
                point.y = node.y;
                allNodeList.Add(point);
                node = node.parent;
            }

            int index = 0;//用来标示当前检测点

            while (index < allNodeList.Count - 1)
            {
                int turnX = (int)allNodeList[index].x;
                int turnY = (int)allNodeList[index].y;

                //分段平滑,不再从最后一个点开始
                for (int i = index + NUM_PER_SESS; i > index; i -= 1)
                {
                    //防止越界
                    if (i > allNodeList.Count - 1)
                    {
                        i = allNodeList.Count - 1;
                    }

                    Point2 point = allNodeList[i];

                    //连通就开始删除点

                    bool isBlock = CheckBlock(turnX, turnY, point.x, point.y);
                    pointPool.PushAllUsedObject();

                    if (!isBlock)
                    {
                        allNodeList.RemoveRange((index + 1), (i - index - 1));
                        index++;
                        break;
                    }

                    //达了末尾，没找到可去除点，进入下一次检测
                    if (i == index + 1)
                    {
                        index++;
                        break;
                    }
                }   //end of for
            }   //end of while

            return allNodeList;
        }

        /// <summary>
        /// 获取路径节点的数量
        /// </summary>
        private int GetAllNodeCount(Node node)
        {
            int count = 0;
            while (node != null)
            {
                count++;
                node = node.parent;
            }
            return count;
        }

        /// <summary>
        /// 检测是否有阻碍
        /// </summary>
        private bool CheckBlock(int startX, int startY, int endX, int endY)
        {
            if (startX == endX && startY == endY)
                return false;

            //--0:数据准备
            Point2 startPoint = pointPool.Pop().InitData(startX, startY);
            Point2 endPoint = pointPool.Pop().InitData(endX, endY);

            int minY = Math.Min(startPoint.y, endPoint.y);
            int maxY = Math.Max(startPoint.y, endPoint.y);

            int minX = Math.Min(startPoint.x, endPoint.x);
            int maxX = Math.Max(startPoint.x, endPoint.x);

            Dictionary<int, Point2> pointList = new Dictionary<int, Point2>();//交点集合

            //--1 添加起点和终点
            AddPoint2(pointList, startPoint.x, startPoint.y);
            AddPoint2(pointList, endPoint.x, endPoint.y);

            //--2 填充交点
            if (endPoint.x == startPoint.x)
            {
                //X轴平移
                int x = endPoint.x;
                for (int y = minY; y <= maxY; y++)
                {
                    AddPoint2(pointList, x, y);
                }
            }
            else if (endPoint.y == startPoint.y)
            {
                //Y轴平移
                int y = endPoint.y;
                for (int x = minX; x <= maxX; x++)
                {
                    AddPoint2(pointList, x, y);
                }
            }
            else
            {
                //使用直线的斜率公式计算交点
                float x1 = startX + 0.5f;
                float y1 = startY + 0.5f;
                float x2 = endX + 0.5f;
                float y2 = endY + 0.5f;

                long dx = (long)(x2 * 10 - x1 * 10);
                long dy = (long)(y2 * 10 - y1 * 10);

                //1:求出直线公式y=kx+b
                Fraction K = new Fraction(dy, dx);
                Fraction K2 = new Fraction(dx, dy);
                Fraction B = new Fraction((long)(y1 * 10000), 10000) - K * new Fraction((long)(x1 * 10000), 10000);

                Fraction tempFrac = new Fraction();

                //2:先进行x方向递增，算出位置
                for (int x = minX; x <= maxX; x++)
                {
                    float folydY = ((K * (tempFrac.InitData(x)) + B).ToFloat());
                    int y = (int)folydY;

                    if (y >= minY && y <= maxY)
                    {
                        //证明：不在线段的延长线上
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
                    return true;
            }

            return false;
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
            Point2 p = pointPool.Pop().InitData(x, y);
            if (!pointList.ContainsKey(p.Key))
            {
                pointList.Add(p.Key, p);
            }
            else
            {
                pointPool.Push(p);
            }
        }
    }
}