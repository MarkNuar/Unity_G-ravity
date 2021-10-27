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

        private SystemData _systemData = null;
        private List<CBodyUIHelper> _helpers = new List<CBodyUIHelper>();
        
        private int _currentCBodyIndex;

        [SerializeField] private TMP_InputField inputSystemName = null;
        
        [SerializeField] private TMP_InputField inputCBodyName = null;
        [SerializeField] private ColorPicker colorPicker = null;
        [SerializeField] private Button cBodyColorButton = null;

        [SerializeField] private RectTransform colorHandle = null;
        
        public int maxCBodyElements = 10;

        [SerializeField] private CameraController _cameraController;
        
        private void Start()
        {
            // load saved CBodies
            var systemSettings = GameManager.Instance.LoadSystem("varudia");
            if (systemSettings != null)
            {
                _systemData = systemSettings;
                inputSystemName.text = _systemData.systemName;
                foreach (var cb in _systemData.GetCBodies())
                {
                    _currentCBodyIndex = cb.index;
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
            if (_systemData.GetCBodies().Count < maxCBodyElements + 1)
            {
                if (!loaded)
                {
                    _currentCBodyIndex = _systemData.AddNewCBody();
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
                helper.bodyName.text = _systemData.GetCBodyAtIndex(_currentCBodyIndex).name;
                var curCBodyData = _systemData.GetCBodies()[_currentCBodyIndex];
                helper.selectButton.onClick.AddListener(() => OpenContextualMenu(curCBodyData.index));
                Debug.Log(_currentCBodyIndex);
                
                _helpers.Add(helper);
                
                DeselectCurrentCBody();
            }
            else
            {
                Debug.LogError("Too many elements");
            }
        }

        private void OpenContextualMenu(int currentCBodyIndex)
        {
            _currentCBodyIndex = currentCBodyIndex;
            SelectCurrentCBody();
            ShowPanel(1);
        }

        public void OpenEditMenu()
        {
            // Todo: disable camera movement, and place it near the planet
            Vector3 pos = _helpers[_currentCBodyIndex].CBodyUIElement.transform.position;
            
            // var main = Camera.main;
            // if (main is { })
            // {
            //     var transform1 = main.transform;
            //     transform1.position = new Vector3(pos.x,transform1.position.y, transform1.position.z);
            // }
            _cameraController.LockCamAt(pos);
            
            // end todo
            Debug.Log("Begin editing cbody n. " + _currentCBodyIndex);

            DisableCBodyButtons();
            
            inputCBodyName.text = _systemData.GetCBodies()[_currentCBodyIndex].name;
            
            colorPicker.CurrentColor = (_systemData.GetCBodies()[_currentCBodyIndex].appearance.color);

            SetButtonColor(cBodyColorButton, _systemData.GetCBodies()[_currentCBodyIndex].appearance.color);
            
            DeselectCurrentCBody();

            ShowPanel(2);
        }

        public void CloseEditMenu()
        {
            _cameraController.FreeCam();
            EnableCBodyButtons();
            SelectCurrentCBody();
            ShowPanel(1);
        }

        public void DeleteCBody()
        {
            DestroyCurrentCBody();
            _currentCBodyIndex = -1;
            ShowPanel(0);
        }
        
        public void CloseContextualMenu()
        {
            DeselectCurrentCBody();
            _currentCBodyIndex = -1;
            ShowPanel(0);
        }

        public void SetCBodyName(string cbName)
        {
            _systemData.GetCBodies()[_currentCBodyIndex].name = cbName;
        }

        public void BeginPickColor()
        {
            colorPicker.CurrentColor = _systemData.GetCBodies()[_currentCBodyIndex].appearance.color;
            colorPicker.onValueChanged.AddListener(SetCBodyColor);
            OverlayPanel(2,true);
        }

        private void SetCBodyColor(Color color)
        {
            _systemData.GetCBodies()[_currentCBodyIndex].appearance.color = color;
            SetButtonColor(cBodyColorButton, color);
        }

        public void EndPickColor()
        {
            colorPicker.onValueChanged.RemoveListener(SetCBodyColor);
            OverlayPanel(2, false);
        }

        private void DestroyCurrentCBody()
        {
            Destroy(_helpers[_currentCBodyIndex].CBodyUIElement);
            _helpers.RemoveAt(_currentCBodyIndex);
            _systemData.RemoveCBodyAtIndex(_currentCBodyIndex);
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

        private void SetButtonColor(Button button, Color color)
        {
            var buttonColors = button.colors;
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
                    _helpers[i].HideSelectionMesh();
            }
        }

        private void DeselectCurrentCBody()
        {
            _helpers[_currentCBodyIndex].HideSelectionMesh();
        }

        private void DisableCBodyButtons()
        {
            foreach (var help in _helpers)
            {
                help.selectButton.enabled = false;
            }
        }
        
        private void EnableCBodyButtons()
        {
            foreach (var help in _helpers)
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
