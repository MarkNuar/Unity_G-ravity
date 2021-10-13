using System.Collections.Generic;
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
        // Reference to the scrollable container
        public RectTransform contentRef;
        // Prefab of ui list element
        public GameObject cBodyPrefab;

        private SystemData _systemData = null;
        //private readonly List<CBodyAppearanceSettings> _cBodyAppearanceList = new List<CBodyAppearanceSettings>();
        
        private int _currentCBodyIndex;
        
        private GameObject _currentUIListElement = null;

        [SerializeField] private TMP_InputField inputSystemName = null;
        
        [SerializeField] private TMP_InputField inputCBodyName = null;
        [SerializeField] private ColorPicker colorPicker = null;
        [SerializeField] private Button cBodyColorButton = null;

        [SerializeField] private RectTransform colorHandle = null;
        
        public int maxCBodyElements = 10;
        
        private void Start()
        {
            // load saved CBodies
            var systemSettings = GameManager.Instance.LoadSystem("Varudia");
            if (systemSettings != null)
            {
                _systemData = systemSettings;
                inputSystemName.text = _systemData.systemName;
                foreach (var cb in _systemData.GetCBodies())
                {
                    _currentCBodyIndex = cb.index;
                    CompleteCBodyCreation();
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

        public void BeginCBodyCreation()
        {
            if (_systemData.GetCBodies().Count < maxCBodyElements)
            {
                ShowPanel(1);
                // todo: show planet in the canvas
                _currentCBodyIndex = _systemData.AddNewCBody();

                inputCBodyName.text = _systemData.GetCBodies()[_currentCBodyIndex].name;
                
                colorPicker.CurrentColor = (_systemData.GetCBodies()[_currentCBodyIndex].appearance.color);

                SetButtonColor(cBodyColorButton, _systemData.GetCBodies()[_currentCBodyIndex].appearance.color);
            }
            else
            {
                Debug.Log("Too many elements");
            }
        }

        private void BeginCBodyEditing(CBodyData cBodyData, GameObject currentListEl)
        {
            _currentUIListElement = currentListEl;
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

        public void PickColor()
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
        
        public void CancelCBodyCreationOrEditing()
        {
            _currentUIListElement = null;
            _currentCBodyIndex = -1;
            ShowPanel(0);
        }
        
        public void CompleteCBodyCreation()
        {
            GameObject listElement = null;
            CBodyUIElement listElementUI = null;
            if (_currentUIListElement == null)
            {
                listElement = Instantiate(cBodyPrefab, transform, true);
                if (listElement.transform is RectTransform elRect)
                {
                    elRect.SetParent(contentRef);
                    elRect.localScale = Vector3.one;
                }
                var currentCBody = _systemData.GetCBodies()[_currentCBodyIndex];
                listElementUI = listElement.GetComponent<CBodyUIElement>();
                listElementUI.deleteButton.onClick.AddListener
                    (() => RemoveElement(currentCBody, listElement));
                listElementUI.editButton.onClick.AddListener
                    (() => BeginCBodyEditing(currentCBody, listElement));
            }
            else
            {
                listElementUI = _currentUIListElement.GetComponent<CBodyUIElement>();
            }

            listElementUI.bodyName.text = _systemData.GetCBodies()[_currentCBodyIndex].name;
            listElementUI.image.color = _systemData.GetCBodies()[_currentCBodyIndex].appearance.color;
            SetButtonColor(cBodyColorButton, _systemData.GetCBodies()[_currentCBodyIndex].appearance.color);
            
            
            _currentCBodyIndex = -1;
            _currentUIListElement = null;
            
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
