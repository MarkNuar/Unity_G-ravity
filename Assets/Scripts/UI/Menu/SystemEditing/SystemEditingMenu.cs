using System;
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
        public float height = 80;
        public float width = 60;
        
        private readonly List<CBodyAppearanceSettings> _cBodyAppearanceList = new List<CBodyAppearanceSettings>();
        
        private CBodyAppearanceSettings _currentCBodyAppearance;
        private GameObject _currentUIListElement = null;
        private int _currentUIListElementPos;

        [SerializeField] private TMP_InputField inputField = null;
        [SerializeField] private ColorPicker colorPicker = null;
        [SerializeField] private Button cBodyColorButton = null;

        [SerializeField] private RectTransform colorHandle = null;
        
        public int maxCBodyElements = 10;
        
        private void Start()
        {
            // load saved CBodies
            var systemSettings = GameManager.Instance.LoadSystem("test_system");
            if (systemSettings != null)
            {
                foreach (var a in systemSettings.Appearances)
                {
                    _currentCBodyAppearance = a;
                    CompleteCBodyCreation();
                }
            }
            
            // sometimes the color handle is spawned in the wrong position
            colorHandle.anchoredPosition = new Vector2(0, 0);
        }

        public void BeginCBodyCreation()
        {
            if (_cBodyAppearanceList.Count < maxCBodyElements)
            {
                ShowPanel(1);
                // todo: show planet in the canvas
                _currentCBodyAppearance = new CBodyAppearanceSettings();
                _currentCBodyAppearance.Init();
                
                inputField.text = _currentCBodyAppearance.Name;
                
                colorPicker.CurrentColor = (_currentCBodyAppearance.Color);
                //colorPicker.ToggleColorSliders();
                
                SetButtonColor(cBodyColorButton, _currentCBodyAppearance.Color);
            }
            else
            {
                Debug.Log("Too many elements");
            }
        }

        private void BeginCBodyEditing(CBodyAppearanceSettings settings, GameObject currentListEl)
        {
            _currentUIListElement = currentListEl;
            _currentUIListElementPos = _cBodyAppearanceList.LastIndexOf(settings);
            ShowPanel(1);

            _currentCBodyAppearance = settings;
            
            inputField.text = _currentCBodyAppearance.Name;
            
            colorPicker.CurrentColor = (_currentCBodyAppearance.Color);
            //colorPicker.ToggleColorSliders();
            
            SetButtonColor(cBodyColorButton, _currentCBodyAppearance.Color);
        }

        public void SetCBodyName(string cbName)
        {
            _currentCBodyAppearance.Name = cbName;
        }

        public void PickColor()
        {
            //todo colorPicker.AssignColor(_currentCBodyAppearance.Color);
            colorPicker.onValueChanged.AddListener(SetCBodyColor);
            OverlayPanel(2,true);
        }

        private void SetCBodyColor(Color color)
        {
            _currentCBodyAppearance.Color = color;
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
            _currentCBodyAppearance = null;
            ShowPanel(0);
        }
        
        public void CompleteCBodyCreation()
        {
            GameObject listElement;
            if (_currentUIListElement == null)
            {
                listElement = Instantiate(cBodyPrefab, transform, true);
                if (listElement.transform is RectTransform elRect)
                {
                    elRect.SetParent(contentRef);
                    elRect.localScale = Vector3.one;
                    elRect.sizeDelta = new Vector2(width, height);
                }
                _cBodyAppearanceList.Add(_currentCBodyAppearance);
                var appearanceSettings = _cBodyAppearanceList[_cBodyAppearanceList.Count - 1];
                listElement.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => RemoveElement(appearanceSettings, listElement));
                listElement.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => BeginCBodyEditing(appearanceSettings, listElement));
            }
            else
            {
                listElement = _currentUIListElement;
                _cBodyAppearanceList[_currentUIListElementPos] = _currentCBodyAppearance;
            }

            listElement.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = _currentCBodyAppearance.Name;
            listElement.transform.GetChild(2).GetComponent<Image>().color = _currentCBodyAppearance.Color;
            
            _currentCBodyAppearance = null;
            _currentUIListElement = null;
            _currentUIListElementPos = 0;
            SetButtonColor(cBodyColorButton, Color.white);
            
            ShowPanel(0);
        }

        private void RemoveElement(CBodyAppearanceSettings settings,GameObject element)
        {
            _cBodyAppearanceList.Remove(settings);
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
            var s = new CBodySystemSettings
            {
                SystemName = "test_system",
                Appearances = _cBodyAppearanceList
            };
            GameManager.Instance.SaveSystem(s);
        }
    }
}
