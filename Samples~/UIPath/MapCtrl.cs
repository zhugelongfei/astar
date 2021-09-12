using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Lonfee.AStar.Sample
{
    public class MapCtrl : IAstarDebug
    {
        public static string MAP_DIR = Application.dataPath + "/AStarMaps/";
        private EUserCtrlStatus ctrlState;
        private UINode[,] nodeArray;
        private AStarCore astar;

        private int line;
        private int col;

        private UINode startNode;
        private UINode tarNode;

        public MapCtrl(int line, int col, UINode[,] nodeArr)
        {
            this.line = line;
            this.col = col;
            this.nodeArray = nodeArr;
            astar = new AStarCore(IsBlock, CapacitySize.Default, true, this);

            for (int i = 0; i < line; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    nodeArray[i, j].SetClickCB(OnClk_Node);
                }
            }
        }

        public void SetCtrlState(EUserCtrlStatus state)
        {
            ctrlState = state;
        }

        public void OnClk_Node(UINode node)
        {
            if (ctrlState == EUserCtrlStatus.SetTarget)
            {
                if (tarNode != null)
                    tarNode.SetTag(ENodeTag.Normal);
                tarNode = null;

                tarNode = node;
                tarNode.SetTag(ENodeTag.Target);
            }
            else if (ctrlState == EUserCtrlStatus.SetStart)
            {
                if (startNode != null)
                    startNode.SetTag(ENodeTag.Normal);
                startNode = null;

                startNode = node;
                startNode.SetTag(ENodeTag.Start);
            }
            else if (ctrlState == EUserCtrlStatus.SetBlock)
            {
                if (node == startNode)
                    startNode = null;
                if (node == tarNode)
                    tarNode = null;
                node.SetTag(node.Tag != ENodeTag.Block ? ENodeTag.Block : ENodeTag.Normal);
            }
        }

        public List<Vector3> ShowPath(bool useFolyd, float unitRadius)
        {
            List<Vector3> debugLine = new List<Vector3>();

            if (startNode == null || tarNode == null)
            {
                Debug.LogError("Please set start and target befor find path");
                return debugLine;
            }

            List<Point2> path = astar.FindPath(startNode.x, startNode.y, tarNode.x, tarNode.y);
            if (path != null)
            {
                // src path
                for (int i = 1; i < path.Count - 1; i++)
                {
                    Point2 p = path[i];
                    nodeArray[p.y, p.x].SetTag(ENodeTag.Path);
                }

                if (useFolyd)
                {
                    // moothness
                    astar.FolydMoothnessPath(path, unitRadius);
                    for (int i = 1; i < path.Count - 1; i++)
                    {
                        Point2 p = path[i];
                        nodeArray[p.y, p.x].SetTag(ENodeTag.Folyd);
                    }
                }

                // debug path
                foreach (var item in path)
                {
                    debugLine.Add(GetUINode(item).Position);
                }
            }
            else
            {
                Debug.LogError("Can not find the path");
            }

            return debugLine;
        }

        private UINode GetUINode(Point2 p)
        {
            return nodeArray[p.y, p.x];
        }

        public void ClearNodeTag()
        {
            for (int i = 0; i < line; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    UINode curNode = nodeArray[i, j];
                    if (curNode.Tag == ENodeTag.Start || curNode.Tag == ENodeTag.Target || curNode.Tag == ENodeTag.Block)
                        continue;
                    nodeArray[i, j].SetTag(ENodeTag.Normal);
                }
            }
        }

        private bool IsBlock(int x, int y)
        {
            if (x < 0 || x >= col)
                return true;

            if (y < 0 || y >= line)
                return true;

            return nodeArray[y, x].Tag == ENodeTag.Block;
        }

        public void SetPath(ICollection<Point2> nodeList)
        {

        }

        public void SetClosePointCollection(ICollection<Point2> nodeList)
        {
            foreach (var item in nodeList)
            {
                UINode curNode = nodeArray[item.y, item.x];

                if (curNode.Tag == ENodeTag.Start || curNode.Tag == ENodeTag.Target)
                    continue;

                curNode.SetTag(ENodeTag.Close);
            }
        }

        public void SetOpenPointCollection(ICollection<Point2> nodeList)
        {
            foreach (var item in nodeList)
            {
                nodeArray[item.y, item.x].SetTag(ENodeTag.Open);
            }
        }

        public void Save(int id)
        {
            if (!Directory.Exists(MAP_DIR))
                Directory.CreateDirectory(MAP_DIR);

            string fileName = MAP_DIR + id.ToString() + ".bytes";
            using (BinaryWriter bw = new BinaryWriter(new FileStream(fileName, FileMode.Create)))
            {
                // 1: int id
                bw.Write(System.BitConverter.GetBytes(id));

                // 2: int line
                bw.Write(System.BitConverter.GetBytes(line));

                // 3: int col
                bw.Write(System.BitConverter.GetBytes(col));

                // 4: resover
                bw.Write(new byte[64]);

                for (int i = 0; i < line; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        UINode curNode = nodeArray[i, j];

                        bw.Write(System.BitConverter.GetBytes((int)curNode.Tag));
                    }
                }
            };
        }

        public void Load(int id)
        {
            string fileName = MAP_DIR + id.ToString() + ".bytes";
            if (!File.Exists(fileName))
                return;

            using (BinaryReader br = new BinaryReader(new FileStream(fileName, FileMode.Open)))
            {
                // 1: int id
                id = br.ReadInt32();

                // 2: int line
                line = br.ReadInt32();

                // 3: int col
                col = br.ReadInt32();

                // 4: resover
                br.ReadBytes(64);

                for (int i = 0; i < line; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        UINode curNode = nodeArray[i, j];

                        ENodeTag tag = (ENodeTag)br.ReadInt32();
                        curNode.SetTag(tag);

                        if (tag == ENodeTag.Start)
                            startNode = curNode;

                        if (tag == ENodeTag.Target)
                            tarNode = curNode;
                    }
                }
            };
        }
    }
}