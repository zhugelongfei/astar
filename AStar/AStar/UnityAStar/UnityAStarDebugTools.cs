using UnityEngine;
using AStar;
using System.Collections.Generic;

public class UnityAStarDebugTools : IAstarDebug
{
    private bool isUseRedCube = false;
    private bool isUseBlackCube = false;

    private const float RED_CUBE_SCALE = 0.3f;          //红色方块的大小
    private const float BLACK_CUBE_SCALE = 0.5f;        //黑色方块的大小
    private GameObject cubeObjRoot;                     //方块的挂载点
    private List<Vector3> findPath;

    public class CubeItem : Pool.IPoolItem
    {
        private GameObject cube;

        public CubeItem()
        {
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }

        public void DestroyObject()
        {
            Object.Destroy(cube);
        }

        public void PopCallBack()
        {
            cube.SetActive(true);
        }

        public void PushCallBack()
        {
            cube.name = "Idle";
            cube.SetActive(false);
        }

        public CubeItem SetInfo(Transform parent, Vector3 position, float scale, Color color)
        {
            cube.transform.parent = parent;
            cube.transform.localPosition = position;
            cube.transform.localScale = new Vector3(scale, scale, scale);
            cube.renderer.material.color = color;

            return this;
        }

        public void SetName(string name)
        {
            cube.name = name;
        }
    }

    private Pool.SimpleObjectPool<CubeItem> pool = new Pool.SimpleObjectPool_DefaultCreateItemFunction<CubeItem>(int.MaxValue);

    public void Init()
    {
        if (cubeObjRoot == null)
            cubeObjRoot = new GameObject();
        cubeObjRoot.name = "Path Root";

        pool.PushAllUseItem();
    }

    /// <summary>
    /// 创建黑色方块（曾经探索果的区域）
    /// </summary>
    public void SetCloseNodeList(ICollection<Node> nodeList)
    {
        if (!isUseBlackCube)
            return;

        foreach (var item in nodeList)
        {
            CubeItem cube = pool.Pop().SetInfo(cubeObjRoot.transform, new Vector3(item.x + 0.5f, 0, item.y + 0.5f), BLACK_CUBE_SCALE, Color.black);
            cube.SetName("Black");
        }
    }

    /// <summary>
    /// 创建红色方块（行走路径）
    /// </summary>
    public void SetPathNode(Node node)
    {
        if (!isUseRedCube)
            return;

        while (node != null)
        {
            CubeItem item = pool.Pop().SetInfo(cubeObjRoot.transform, new Vector3(node.x + 0.5f, 0.5f, node.y + 0.5f), RED_CUBE_SCALE, Color.red);
            item.SetName("Red_G-" + node.g + " H-" + node.h + " F-" + node.f);
            node = node.parent;
        }
    }

    public void SetFindPath(List<Vector3> path)
    {
        findPath = path;
    }

    public void UpdatePath()
    {
        if (findPath != null)
        {
            Vector3 stPos = Vector3.zero;
            Vector3 tarPos = Vector3.zero;

            for (int i = 0; i < findPath.Count; i++)
            {
                if (i + 1 < findPath.Count)
                {
                    stPos = findPath[i] + new Vector3(0.5f, 0, 0.5f);
                    tarPos = findPath[i + 1] + new Vector3(0.5f, 0, 0.5f);
                    Debug.DrawLine(stPos, tarPos);
                }
            }
        }
    }

    public void Log(string str)
    {
        Debug.LogError(str);
    }
}