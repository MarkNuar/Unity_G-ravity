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
        private int _currentCBodyIndex = -1;

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
                Debug.Log(_systemData.cBodies[_currentCBodyIndex].physics.radius);
                
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
            
            
            preview.selectButton.onClick.AddListener(() => OpenContextualMenu(_systemData.cBodies.IndexOf(curCBodyData)));
            preview.velocityArrow.onDrag.AddListener((d, p) => curCBodyData.physics.initialVelocity = 
                d *((ParameterValues.maxVelocity - ParameterValues.minVelocity)*p + ParameterValues.minVelocity));
            preview.positionDrag.onDrag.AddListener(v => curCBodyData.physics.initialPosition = v);
            
            
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

            SetDragHandlePosition();
            SetArrowHeadPosition();

            // set gravity and radius
            UpdateContextualSliders();
            
            ShowPanel(1);
        }

        public void SetCBodyRadius()
        {
            //Debug.Log(radiusSlider.value);
            // todo test
            _systemData.cBodies[_currentCBodyIndex].physics.radius = 
                radiusSlider.value * (ParameterValues.maxRadius - ParameterValues.minRadius) + ParameterValues.minRadius;
        }

        public void SetCBodyGravity()
        {
            //Debug.Log(gravitySlider.value);
            // todo test
            _systemData.cBodies[_currentCBodyIndex].physics.surfaceGravity = 
                gravitySlider.value * (ParameterValues.maxGravity - ParameterValues.minGravity) + ParameterValues.minGravity;
        }
        
        public void OpenEditMenu(bool fromCreation)
        {
            Vector3 pos = _cBodyPreviews[_currentCBodyIndex].cBody.transform.position;
            var cBodyRadius = _systemData.cBodies[_currentCBodyIndex].physics.radius;
            
            cameraController.LockCamAt(pos, cBodyRadius, fromCreation);
            
            DisableCBodyButtons();
            
            inputCBodyName.text = _systemData.cBodies[_currentCBodyIndex].name;
            SetButtonColor(cBodyColorButton, _systemData.cBodies[_currentCBodyIndex].appearance.color);

            HideCurrentCBodySelectionHUD();

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
                _currentCBodyIndex = -1;
            }
        }

        private void HideCurrentCBodySelectionHUD()
        {
            if (_currentCBodyIndex != -1)
            {
                _cBodyPreviews[_currentCBodyIndex].HideSelectionHUD();
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
            var percent = 
                (_systemData.cBodies[_currentCBodyIndex].physics.initialVelocity.magnitude - ParameterValues.minVelocity) /
                          (ParameterValues.maxVelocity - ParameterValues.minVelocity);
            _cBodyPreviews[_currentCBodyIndex].velocityArrow.SetArrowHeadPosition(
                _systemData.cBodies[_currentCBodyIndex].physics.initialVelocity.normalized,percent);
        }

        private void SetDragHandlePosition()
        {
            _cBodyPreviews[_currentCBodyIndex].positionDrag.ResetDragHandlePosition();
        }

        private void UpdateContextualSliders()
        {
            var r = (_systemData.cBodies[_currentCBodyIndex].physics.radius - ParameterValues.minRadius) /
                    (ParameterValues.maxRadius - ParameterValues.minRadius);
            var g = (_systemData.cBodies[_currentCBodyIndex].physics.surfaceGravity - ParameterValues.minGravity) /
                    (ParameterValues.maxGravity - ParameterValues.minGravity);
            
            radiusSlider.value = r;
            radiusSlider.onValueChanged.Invoke(r);
            gravitySlider.value = g;
            gravitySlider.onValueChanged.Invoke(g);
        }

        private void OnDestroy()
        {
            GameManager.Instance.SaveSystem(_systemData);
        }
    }
}
