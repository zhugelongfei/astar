using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lonfee.AStar.Sample
{
    public class Main : MonoBehaviour
    {
        #region UI

        public Transform gridTran;
        public Transform nodeTran;

        // map info
        public InputField fileNameIF;
        public Button saveBtn;
        public Button loadBtn;

        // folyd
        public Toggle useFolydToggle;
        public Slider unitRadiusSlider;
        public Text unitRadiusText;
        public GameObject unitRadiusObj;

        // help
        public GameObject helpPanel;
        public Button closeHelpBtn;
        public Button helpBtn;

        #endregion

        public int col;
        public int line;

        private MapCtrl mapCtrl;

        private List<Vector3> debugPath;

        private void Awake()
        {
            loadBtn.onClick.AddListener(OnClk_Load);
            saveBtn.onClick.AddListener(OnClk_Save);
            unitRadiusSlider.onValueChanged.AddListener((val) => { unitRadiusText.text = (val * 0.01f).ToString(); });
            helpBtn.onClick.AddListener(() => { helpPanel.SetActive(!helpPanel.activeInHierarchy); });
            closeHelpBtn.onClick.AddListener(() => { helpPanel.SetActive(false); });
            useFolydToggle.onValueChanged.AddListener((isOn) => { unitRadiusObj.SetActive(isOn); });

            unitRadiusObj.SetActive(useFolydToggle.isOn);
            unitRadiusText.text = (unitRadiusSlider.value * 0.01f).ToString();
            helpPanel.SetActive(false);
            useFolydToggle.isOn = true;
        }

        private void OnClk_Load()
        {
            mapCtrl.Load(int.Parse(fileNameIF.text));
        }

        private void OnClk_Save()
        {
            mapCtrl.Save(int.Parse(fileNameIF.text));
        }

        void Start()
        {
            // 1: init UI
            GridLayoutGroup glp = gridTran.GetComponent<GridLayoutGroup>();
            glp.constraintCount = col;
            nodeTran.gameObject.SetActive(false);

            // 2: create ui node
            UINode[,] nodeArray = new UINode[line, col];
            for (int i = 0; i < line; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    GameObject newObj = Instantiate(nodeTran.gameObject, gridTran);
                    newObj.SetActive(true);
                    UINode node = new UINode(j, i, newObj);
                    nodeArray[i, j] = node;
                }
            }

            // 3: init ctrl and astar
            mapCtrl = new MapCtrl(line, col, nodeArray);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                mapCtrl.ClearNodeTag();
                debugPath = mapCtrl.ShowPath(useFolydToggle.isOn, unitRadiusSlider.value * 0.01f);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                mapCtrl.SetCtrlState(EUserCtrlStatus.SetStart);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                mapCtrl.SetCtrlState(EUserCtrlStatus.SetTarget);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                mapCtrl.SetCtrlState(EUserCtrlStatus.SetBlock);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                mapCtrl.ClearNodeTag();
            }

            if (debugPath != null && debugPath.Count >= 2)
            {
                for (int i = 0; i < debugPath.Count - 1; i++)
                {
                    Debug.DrawLine(debugPath[i], debugPath[i + 1], Color.red);
                }

                Debug.DrawLine(debugPath[0], debugPath[debugPath.Count - 1], Color.white);
            }
        }

    }
}