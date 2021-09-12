using UnityEngine;
using UnityEngine.UI;

namespace Lonfee.AStar.Sample
{
    public class UINode
    {
        public int x;
        public int y;

        private GameObject obj;
        private Button btn;
        private Text text;
        private Text indexText;
        private Image img;

        private ENodeTag mTag;
        private System.Action<UINode> clickCB;

        public Vector3 Position
        {
            get { return obj.transform.position; }
        }

        public ENodeTag Tag
        {
            get { return mTag; }
        }

        public UINode(int x, int y, GameObject obj)
        {
            this.x = x;
            this.y = y;
            this.obj = obj;
            obj.name = string.Format("{0}_{1}", x, y);

            // find ui
            btn = obj.GetComponent<Button>();
            text = obj.transform.Find("Text").GetComponent<Text>();
            indexText = obj.transform.Find("Index").GetComponent<Text>();
            img = obj.transform.Find("Image").GetComponent<Image>();

            indexText.text = string.Format("{0}, {1}", x, y);
            btn.onClick.AddListener(OnClk_Btn);
        }

        public void SetClickCB(System.Action<UINode> clickCB)
        {
            this.clickCB = clickCB;
        }

        private void OnClk_Btn()
        {
            clickCB?.Invoke(this);
        }

        public void SetTag(ENodeTag tag)
        {
            this.mTag = tag;
            RefreshUI();
        }

        public void RefreshUI()
        {
            switch (mTag)
            {
                case ENodeTag.Normal:
                    img.color = Color.white;
                    break;
                case ENodeTag.Block:
                    img.color = Color.gray;
                    break;
                case ENodeTag.Path:
                    img.color = Color.green;
                    break;
                case ENodeTag.Start:
                    img.color = Color.yellow;
                    break;
                case ENodeTag.Target:
                    img.color = Color.cyan;
                    break;
                case ENodeTag.Folyd:
                    img.color = Color.magenta;
                    break;
                case ENodeTag.Open:
                    img.color = Color.blue;
                    break;
                case ENodeTag.Close:
                    img.color = Color.red;
                    break;
                default:
                    break;
            }

            if (mTag == ENodeTag.Normal || mTag == ENodeTag.Block)
                text.text = string.Empty;
            else
                text.text = mTag.ToString();
        }

    }
}