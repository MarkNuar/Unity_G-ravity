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
        // For switching panels
        public List<GameObject> panels;

        private SystemData _systemData = null;
        //private readonly List<CBodyAppearanceSettings> _cBodyAppearanceList = new List<CBodyAppearanceSettings>();
        
        private int _currentCBodyIndex;

        [SerializeField] private TMP_InputField inputSystemName = null;
        
        [SerializeField] private TMP_InputField inputCBodyName = null;
        [SerializeField] private ColorPicker colorPicker = null;
        [SerializeField] private Button cBodyColorButton = null;

        [SerializeField] private RectTransform colorHandle = null;
        
        public int maxCBodyElements = 10;
        
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
                GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tempCube.transform.position = _systemData.GetCBodyAtIndex(_currentCBodyIndex).physics.initialPosition;
                tempCube.GetComponent<MeshRenderer>().material.color = _systemData.GetCBodyAtIndex(_currentCBodyIndex).appearance.color;
                // todo
            }
            else
            {
                Debug.LogError("Too many elements");
            }
            
        }
        
        public void BeginCBodyEditing(CBodyData cBodyData)
        {
            // move camera close to the selected planet
            ShowPanel(1);

            _currentCBodyIndex = cBodyData.index;

            inputCBodyName.text = _systemData.GetCBodies()[_currentCBodyIndex].name;
            
            colorPicker.CurrentColor = (_systemData.GetCBodies()[_currentCBodyIndex].appearance.color);

            SetButtonColor(cBodyColorButton, _systemData.GetCBodies()[_currentCBodyIndex].appearance.color);
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
        
        public void EndCBodyEditing()
        {
            _currentCBodyIndex = -1;
            ShowPanel(0);
        }

        private void RemoveElement(CBodyData cBodyData, GameObject element)
        {
            _systemData.RemoveCBodyAtIndex(cBodyData.index);
            Destroy(element);
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
        
        private void OnDestroy()
        {
            GameManager.Instance.SaveSystem(_systemData);
        }
    }
}
