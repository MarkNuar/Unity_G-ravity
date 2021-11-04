using System;
using System.Collections.Generic;
using CBodies;
using CBodies.Data;
using HSVPicker;
using TMPro;
using UI.Menu.SystemEditing.Preview;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu.SystemEditing
{
    public class SystemEditingMenu : MonoBehaviour
    {
        public GameObject cBodyPreviewPrefab;

        public Shader cBodyPreviewShader;
        // For switching panels
        public List<GameObject> panels;

        // logical view of the planets
        private SystemData _systemData = null;
        // actual view of the planets
        private readonly List<CBodyPreview> _cBodyPreviews = new List<CBodyPreview>();
        
        // Index of the currently selected cbody
        private int _currentCBodyIndex;

        public int maxCBodyElements = 10;
        
        // PHYSICS
        [SerializeField] private Slider radiusSlider = null;
        [SerializeField] private Slider gravitySlider = null;
        
        
        
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
                // Debug.Log(_systemData.cBodies.Count);
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

        private void Update()
        {
            if (_currentCBodyIndex >= 0 && _currentCBodyIndex < _cBodyPreviews.Count)
            {
                Debug.Log(_systemData.cBodies[_currentCBodyIndex].physics.initialPosition);
                Debug.Log(_systemData.cBodies[_currentCBodyIndex].physics.initialVelocity);
            }
        }

        public void SetSystemName(string systemName)
        {
            _systemData.systemName = systemName;
            Debug.Log(systemName);
        }

        public void CreateCBody(bool loaded)
        {
            if (_cBodyPreviews.Count < maxCBodyElements)
            {
                if (!loaded)
                { 
                    _currentCBodyIndex = _systemData.AddNewCBody();
                }
                else
                {
                    // cBody index set by the deserializer
                }
                CreateCBodyPreview();
                
                // DeselectCurrentCBody();
                if (loaded) return;
                SelectCurrentCBody(); 
                OpenEditMenu(true);

            }
            else
            {
                // todo make a visual message
                Debug.LogError("Too many elements");
            }
        }

        private void CreateCBodyPreview()
        {
            GameObject cBodyPreview = Instantiate(cBodyPreviewPrefab);
            
            CBodyPreview preview = cBodyPreview.GetComponent<CBodyPreview>();
            
            //preview.bodyName.text = _systemData.cBodies[_currentCBodyIndex].name;
            CBodyData curCBodyData = _systemData.cBodies[_currentCBodyIndex];
            //Debug.Log(curCBodyData.physics.initialPosition);
            cBodyPreview.transform.position = curCBodyData.physics.initialPosition;
            preview.cBody.transform.localScale = Vector3.one * curCBodyData.physics.radius;
            
            preview.selectButton.onClick.AddListener(() => OpenContextualMenu(_systemData.cBodies.IndexOf(curCBodyData)));
            preview.velocityArrow.onDrag.AddListener((d, p) => curCBodyData.physics.initialVelocity = 
                d *((ParameterValues.maxVelocity - ParameterValues.minVelocity)*p + ParameterValues.minVelocity));
            preview.onDrag.AddListener(v => curCBodyData.physics.initialPosition = v);
            
            
            // Initialize the cBody
            preview.cBody.InitializeCBody(curCBodyData, cBodyPreviewShader);
            // CBody will be updated when values in cBodyData will change
            curCBodyData.Subscribe(preview.cBody);

            _cBodyPreviews.Add(preview);
        }

        // This method sets the current cBody index
        private void OpenContextualMenu(int currentCBodyIndex)
        {
            // avoid unwanted clicks
            if (cameraController.isDragging) return;
            _currentCBodyIndex = currentCBodyIndex;
            SelectCurrentCBody();
            SetArrowHeadPosition();
            
            // todo set gravity and radius
            
            ShowPanel(1);
        }

        public void OpenEditMenu(bool fromCreation)
        {
            Vector3 pos = _cBodyPreviews[_currentCBodyIndex].gameObject.transform.position;
            var cBodyRadius = _systemData.cBodies[_currentCBodyIndex].physics.radius;
            cameraController.LockCamAt(pos, cBodyRadius, fromCreation);
            
            DisableCBodyButtons();
            
            inputCBodyName.text = _systemData.cBodies[_currentCBodyIndex].name;
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
            _systemData.cBodies[_currentCBodyIndex].Unsubscribe();
            Destroy(_cBodyPreviews[_currentCBodyIndex].gameObject);
            _cBodyPreviews.RemoveAt(_currentCBodyIndex);
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
            for (var i = 0; i < _cBodyPreviews.Count; i++)
            {
                if (i == _currentCBodyIndex)
                    _cBodyPreviews[i].SelectCBody();
                else
                    _cBodyPreviews[i].DeselectCBody();
            }
        }

        private void DeselectCurrentCBody()
        {
            if (_currentCBodyIndex != -1)
            {
                _cBodyPreviews[_currentCBodyIndex].DeselectCBody();
            }
        }

        private void HideCurrentCBodySelectionMesh()
        {
            if (_currentCBodyIndex != -1)
            {
                _cBodyPreviews[_currentCBodyIndex].HideSelectionMesh();
            }
        }

        private void DisableCBodyButtons()
        {
            foreach (CBodyPreview help in _cBodyPreviews)
            {
                help.selectButton.enabled = false;
            }
        }
        
        private void EnableCBodyButtons()
        {
            foreach (CBodyPreview help in _cBodyPreviews)
            {
                help.selectButton.enabled = true;
            }
        }

        private void SetArrowHeadPosition()
        {
            // _cBodyPreviews[_currentCBodyIndex].velocityArrow.SetArrowHeadPosition(
            //     _systemData.cBodies[_currentCBodyIndex].physics.initialVelocity + 
            //     _systemData.cBodies[_currentCBodyIndex].physics.initialPosition);
            var percent = 
                (_systemData.cBodies[_currentCBodyIndex].physics.initialVelocity.magnitude - ParameterValues.minVelocity) /
                          (ParameterValues.maxVelocity - ParameterValues.minVelocity);
            _cBodyPreviews[_currentCBodyIndex].velocityArrow.SetArrowHeadPosition(
                _systemData.cBodies[_currentCBodyIndex].physics.initialVelocity.normalized,percent);
            //Debug.Log("Percent: " + percent + ", magnitude: " + _systemData.cBodies[_currentCBodyIndex].physics.initialVelocity.magnitude);
        }

        private void OnDestroy()
        {
            GameManager.Instance.SaveSystem(_systemData);
        }
        
        
        
        // [System.Serializable]
        // public struct Vector {
        //     public Image line;
        //     public Image head;
        //
        //     public void Update (float angle, Vector2 pos, float magnitude, float arrowHeadSize, float thickness) {
        //         line.rectTransform.pivot = new Vector2 (0, 0.5f);
        //         line.rectTransform.eulerAngles = Vector3.forward * angle;
        //         line.rectTransform.localPosition = pos;
        //         line.rectTransform.sizeDelta = new Vector2 (magnitude, thickness);
        //         line.material.SetVector ("_Size", line.rectTransform.sizeDelta);
        //         head.material.SetVector ("_Size", line.rectTransform.sizeDelta);
        //
        //         head.rectTransform.localPosition = pos + (Vector2) line.rectTransform.right * magnitude;
        //         head.rectTransform.eulerAngles = Vector3.forward * angle;
        //
        //         head.rectTransform.localScale = Vector3.one * arrowHeadSize;
        //     }
        //
        //     public void SetActive (bool active) {
        //         line.gameObject.SetActive (active);
        //         head.gameObject.SetActive (active);
        //     }
        // }
    }
}
