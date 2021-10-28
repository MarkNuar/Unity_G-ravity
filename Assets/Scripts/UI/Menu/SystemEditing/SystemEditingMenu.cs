using System.Collections.Generic;
using CBodies;
using CBodies.Data;
using HSVPicker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu.SystemEditing
{
    public class SystemEditingMenu : MonoBehaviour
    {
        public GameObject cBodyPreviewPrefab; 
        
        // For switching panels
        public List<GameObject> panels;

        // logical view of the planets
        private SystemData _systemData = null;
        // actual view of the planets
        private readonly List<CBodyUIHelper> _helpers = new List<CBodyUIHelper>();
        
        // Index of the currently selected cbody
        private int _currentCBodyIndex;

        public int maxCBodyElements = 10;
        
        [SerializeField] private TMP_InputField inputSystemName = null;
        [SerializeField] private TMP_InputField inputCBodyName = null;
        [SerializeField] private ColorPicker colorPicker = null;
        [SerializeField] private Button cBodyColorButton = null;

        [SerializeField] private RectTransform colorHandle = null;
        
        [SerializeField] private CameraController cameraController;
        
        private void Start()
        {
            // load saved CBodies
            SystemData systemSettings = GameManager.Instance.LoadSystem("varudia");
            if (systemSettings != null)
            {
                _systemData = systemSettings;
                Debug.Log(_systemData.cBodies.Count);
                inputSystemName.text = _systemData.systemName;
                foreach (CBodyData cb in _systemData.cBodies)
                {
                    _currentCBodyIndex = _systemData.cBodies.IndexOf(cb);
                    CreateCBody(true);
                }
            }
            else
            {
                _systemData = new SystemData();
            }
            
            // sometimes the color handle is spawned in the wrong position
            colorHandle.anchoredPosition = new Vector2(0, 0);
        }

        public void SetSystemName(string systemName)
        {
            _systemData.systemName = systemName;
            Debug.Log(systemName);
        }

        public void CreateCBody(bool loaded)
        {
            if (_helpers.Count < maxCBodyElements)
            {
                if (!loaded)
                { 
                    _currentCBodyIndex = _systemData.AddNewCBody();
                }
                else
                {
                    // cBody index set by the deserializer
                }
                // todo CBodyDispatcher.GeneratePlanet(_systemData.GetCBodyAtIndex(_currentCBodyIndex));
                // substitute with correct code
                // todo
                // GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                // tempCube.transform.position = _systemData.GetCBodyAtIndex(_currentCBodyIndex).physics.initialPosition;
                // tempCube.GetComponent<MeshRenderer>().material.color = _systemData.GetCBodyAtIndex(_currentCBodyIndex).appearance.color;
                GameObject cBodyPreview = Instantiate(cBodyPreviewPrefab);
                cBodyPreview.transform.position = new Vector3(_currentCBodyIndex * 10, 0, 0);
                
                CBodyUIHelper helper = cBodyPreview.GetComponent<CBodyUIHelper>();
                helper.bodyName.text = _systemData.cBodies[_currentCBodyIndex].name;
                CBodyData curCBodyData = _systemData.cBodies[_currentCBodyIndex];
                helper.selectButton.onClick.AddListener(() => OpenContextualMenu(_systemData.cBodies.IndexOf(curCBodyData)));
                Debug.Log(_currentCBodyIndex);
                
                _helpers.Add(helper);
                
                // DeselectCurrentCBody();
                if (!loaded)
                {
                    SelectCurrentCBody();
                    OpenEditMenu();
                }
            }
            else
            {
                // todo make a visual message
                Debug.LogError("Too many elements");
            }
        }

        // This method sets the current cBody index
        private void OpenContextualMenu(int currentCBodyIndex)
        {
            Debug.Log("Opening contextual menu of index: " + currentCBodyIndex);
            _currentCBodyIndex = currentCBodyIndex;
            SelectCurrentCBody();
            ShowPanel(1);
        }

        public void OpenEditMenu()
        {
            Vector3 pos = _helpers[_currentCBodyIndex].cBodyUIElement.transform.position;
            cameraController.LockCamAt(pos);
            
            DisableCBodyButtons();
            
            inputCBodyName.text = _systemData.cBodies[_currentCBodyIndex].name;
            //colorPicker.CurrentColor = (_systemData.cBodies[_currentCBodyIndex].appearance.color);
            SetButtonColor(cBodyColorButton, _systemData.cBodies[_currentCBodyIndex].appearance.color);

            HideCurrentCBodySelectionMesh();

            ShowPanel(2);
        }
        
        public void CloseAllMenu()
        {
            cameraController.FreeCam();

            EnableCBodyButtons();
            DeselectCurrentCBody();
            ShowPanel(0);
        }

        public void DeleteCBody()
        {
            DestroyCurrentCBody();
            _currentCBodyIndex = -1;
            ShowPanel(0);
        }

        public void SetCBodyName(string cbName)
        {
            // store the new data 
            _systemData.cBodies[_currentCBodyIndex].name = cbName;
            // apply it visually
            _helpers[_currentCBodyIndex].bodyName.text = cbName;
        }

        public void BeginPickColor()
        {
            OverlayPanel(3,true);
            colorPicker.CurrentColor = _systemData.cBodies[_currentCBodyIndex].appearance.color;
            colorPicker.onValueChanged.AddListener(SetCBodyColor);
        }

        private void SetCBodyColor(Color color)
        {
            _systemData.cBodies[_currentCBodyIndex].appearance.color = color;
            SetButtonColor(cBodyColorButton, color);
        }

        public void EndPickColor()
        {
            colorPicker.onValueChanged.RemoveListener(SetCBodyColor);
            OverlayPanel(3, false);
        }

        private void DestroyCurrentCBody()
        {
            Destroy(_helpers[_currentCBodyIndex].cBodyUIElement);
            _helpers.RemoveAt(_currentCBodyIndex);
            _systemData.cBodies.RemoveAt(_currentCBodyIndex);
        }
        
        private void ShowPanel(int position)
        {
            for (var i = 0; i < panels.Count; i ++)
            {
                panels[i].SetActive(i == position);
            }
        }

        private void OverlayPanel(int position, bool overlay)
        {
            panels[position].SetActive(overlay);
        }

        private static void SetButtonColor(Button button, Color color)
        {
            ColorBlock buttonColors = button.colors;
            buttonColors.normalColor = color;
            button.colors = buttonColors;
        }

        private void SelectCurrentCBody()
        {
            for (var i = 0; i < _helpers.Count; i++)
            {
                if (i == _currentCBodyIndex)
                    _helpers[i].SelectCBody();
                else
                    _helpers[i].DeselectCBody();
            }
        }

        private void DeselectCurrentCBody()
        {
            if (_currentCBodyIndex != -1)
            {
                _helpers[_currentCBodyIndex].DeselectCBody();
            }
        }

        private void HideCurrentCBodySelectionMesh()
        {
            if (_currentCBodyIndex != -1)
            {
                _helpers[_currentCBodyIndex].HideSelectionMesh();
            }
        }

        private void DisableCBodyButtons()
        {
            foreach (CBodyUIHelper help in _helpers)
            {
                help.selectButton.enabled = false;
            }
        }
        
        private void EnableCBodyButtons()
        {
            foreach (CBodyUIHelper help in _helpers)
            {
                help.selectButton.enabled = true;
            }
        }
        
        private void OnDestroy()
        {
            GameManager.Instance.SaveSystem(_systemData);
        }
    }
}
