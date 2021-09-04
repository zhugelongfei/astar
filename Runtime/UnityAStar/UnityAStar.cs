//using System.Collections.Generic;
//using UnityEngine;
//using AStar;

//public class UnityAStar
//{
//    private AStarCore core;
//    private UnityAStarDebugTools debugTools = new UnityAStarDebugTools();

//    #region 单例

//    private static UnityAStar myInst;

//    public static UnityAStar Instance
//    {
//        get
//        {
//            if (myInst == null)
//            {
//                myInst = new UnityAStar();
//            }
//            return myInst;
//        }
//    }

//    private UnityAStar() { }

//    #endregion

//    /// <summary>
//    /// 初始化函数
//    /// </summary>
//    /// <param name="checkIsBlockHandle">检测坐标是否是障碍点的委托</param>
//    public void Init(AStarCore.CheckIsBlock checkIsBlockHandle, bool canSlantMove)
//    {
//        core = new AStarCore(checkIsBlockHandle, canSlantMove, debugTools);
//    }

//    public void Update()
//    {
//        debugTools.UpdatePath();
//    }

//    /// <summary>
//    /// 寻路函数
//    /// </summary>
//    /// <param name="startPos">起始点</param>
//    /// <param name="targetPos">目标点</param>
//    /// <param name="smoothPath">是否需要平滑移动</param>
//    /// <returns>所经过的路径点</returns>
//    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos, bool smoothPath)
//    {
//        List<Vector3> pathNodeList = null;

//        Node node = null;
//        bool isFindPath = core.FindPath((int)startPos.x, (int)startPos.z, (int)targetPos.x, (int)targetPos.z, out node);

//        if (isFindPath)
//        {
//            if (smoothPath)
//            {
//                List<Point2> pathPointList = core.FolydmoothnessPath(node);
//                pathNodeList = new List<Vector3>(pathPointList.Count);
//                foreach (var item in pathPointList)
//                {
//                    pathNodeList.Add(new Vector3(item.x, 0, item.y));
//                }
//            }
//            else
//            {
//                pathNodeList = new List<Vector3>();

//                while (node != null)
//                {
//                    pathNodeList.Add(new Vector3(node.x, 0, node.y));
//                    node = node.parent;
//                }
//            }
//        }

//        debugTools.SetFindPath(pathNodeList);

//        return pathNodeList;
//    }
//}