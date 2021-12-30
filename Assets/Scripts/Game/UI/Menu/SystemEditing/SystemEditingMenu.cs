using System;
using System.Collections.Generic;
using CBodies.Settings;
using CBodies.Settings.Shading;
using CBodies.Settings.Shape;
using Game.UI.Menu.SystemEditing.Preview;
using HSVPicker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Physics = CBodies.Settings.Physics.Physics;

namespace Game.UI.Menu.SystemEditing
{
    public class SystemEditingMenu : MonoBehaviour
    {
        public GameObject cBodyPreviewPrefab;

        // For switching panels
        public List<GameObject> panels;

        // logical view of the planets
        private SystemSettings _systemSettings = null;
        // actual view of the planets
        private readonly List<CBodyPreview> _cBodyPreviews = new List<CBodyPreview>();
        
        // Index of the currently selected cbody
        private int _currentCBodyIndex = -1;

        public int maxCBodyElements = 10;
        
        // PHYSICS
        [SerializeField] private Slider radiusSlider = null;
        [SerializeField] private Slider gravitySlider = null;
        [SerializeField] private Slider rotationSlider = null;
        
        // SHAPE & SHADING
        [SerializeField] private TMP_Text systemName = null;
        [SerializeField] private TMP_InputField inputCBodyName = null;
        [SerializeField] private ColorPicker colorPicker = null;
        [SerializeField] private Button cBodyColorButton = null;

        [SerializeField] private RectTransform colorHandle = null;
        
        [SerializeField] private CameraController cameraController;
        
        private void Start()
        {
            // load saved CBodies
            
            var (systemToLoad, isNew) = GameManager.Instance.GetSystemToLoad();
            // // todo: REMOVE FROM BUILD
            // if (systemToLoad == null)
            // {
            //     systemToLoad = "Varudia";
            //     isNew = true;
            // }

            if (systemToLoad != null)
            {
                if (isNew)
                {
                    Debug.Log("New system");
                    _systemSettings = new SystemSettings();
                    SetSystemName(systemToLoad);
                    // Start by adding always a star to the system
                    // TODO : ADD A STAR, NOT A ROCK PLANET AT THE BEGINNING
                    _currentCBodyIndex = _systemSettings.AddNewCBody(CBodySettings.CBodyType.Rocky);
                    CreateCBodyAndPreview();
                }
                else
                {
                    Debug.Log("Loading " + systemToLoad);
                    SystemSettings systemSettings = SystemUtils.Instance.LoadSystem(systemToLoad);
                    if (systemSettings != null)
                    {
                        _systemSettings = systemSettings;
                        systemName.text = _systemSettings.systemName;
                        foreach (CBodySettings cb in _systemSettings.cBodiesSettings)
                        {
                            _currentCBodyIndex = _systemSettings.cBodiesSettings.IndexOf(cb);
                            BeginCBodyCreation(true);
                        }
                        SetSystemName(systemToLoad);
                        //cameraController.LockCamAt(systemSettings.lastCameraPosition, systemSettings.lastCameraZoom - 3, false);
                    }
                    else
                    {
                        Debug.LogError("System not loaded correctly");
                    }
                }
            }

            // sometimes the color handle is spawned in the wrong position
            colorHandle.anchoredPosition = new Vector2(0, 0);
        }

        private void SetSystemName(string sysName)
        {
            _systemSettings.systemName = sysName;
            systemName.text = sysName;
        }

        public void BeginCBodyCreation(bool loaded)
        {
            if (_cBodyPreviews.Count < maxCBodyElements)
            {
                // If CBody loaded from file, no initialization needed
                if (loaded)
                {
                    CreateCBodyAndPreview();
                }
                // If not loaded from file, ask the user for the type of cbody he/she wants to create
                else
                {
                    OpenCBodyTypeSelection();
                }
            }
            else
            {
                // todo make a visual message
                Debug.LogError("Too many elements");
            }
        }

        private void OpenCBodyTypeSelection()
        {
            OverlayPanel(4, true);
        }

        public void CreateCBodyOfType(string type)
        {
            CBodySettings.CBodyType enumType =
                (CBodySettings.CBodyType) Enum.Parse(typeof(CBodySettings.CBodyType), type); 
            _currentCBodyIndex = _systemSettings.AddNewCBody(enumType);
            
            CreateCBodyAndPreview();

            SelectCurrentCBody(); 
            OpenEditMenu(true);
        }

        private void CreateCBodyAndPreview()
        {
            GameObject cBodyPreview = Instantiate(cBodyPreviewPrefab);
            CBodyPreview preview = cBodyPreview.GetComponent<CBodyPreview>();
            
            CBodySettings cBodySettings = GetCurrentCBodySettings();
            cBodySettings.Subscribe(preview.cBody.cBodyGenerator);
            
            preview.selectButton.onClick.AddListener(() => OpenContextualMenu(_systemSettings.cBodiesSettings.IndexOf(cBodySettings)));
            preview.velocityArrow.onDrag.AddListener(UpdateInitialVelocity);
            preview.positionDrag.onDrag.AddListener(UpdateInitialPosition);
            
            _cBodyPreviews.Add(preview);
        }

        private void UpdateInitialVelocity(Vector3 d, float p)
        {
            Physics.PhysicsSettings ps = GetCurrentPhysicsSettings();
            ps.initialVelocity =
                d * ((ParameterValues.maxVelocity - ParameterValues.minVelocity) * p + ParameterValues.minVelocity);
            SetCurrentPhysicsSettings(ps);
        }

        private void UpdateInitialPosition(Vector3 p)
        {
            Physics.PhysicsSettings ps = GetCurrentPhysicsSettings();
            ps.initialPosition = p;
            SetCurrentPhysicsSettings(ps);
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
            Physics.PhysicsSettings ps = GetCurrentPhysicsSettings();
            ps.radius = radiusSlider.value * (ParameterValues.maxRadius - ParameterValues.minRadius) + ParameterValues.minRadius;
            SetCurrentPhysicsSettings(ps);
        }

        public void SetCBodyGravity()
        {
            Physics.PhysicsSettings ps = GetCurrentPhysicsSettings();
            ps.surfaceGravity = gravitySlider.value * (ParameterValues.maxGravity - ParameterValues.minGravity) + ParameterValues.minGravity;
            SetCurrentPhysicsSettings(ps);
        }

        public void SetCBodyRotation()
        {
            Physics.PhysicsSettings ps = GetCurrentPhysicsSettings();
            ps.rotationSpeed = rotationSlider.value * (ParameterValues.maxRotationSpeed - ParameterValues.minRotationSpeed) + ParameterValues.minRotationSpeed;
            SetCurrentPhysicsSettings(ps);
        }
        
        public void OpenEditMenu(bool fromCreation)
        {
            Vector3 pos = _cBodyPreviews[_currentCBodyIndex].cBody.transform.position;
            var cBodyRadius = GetCurrentPhysicsSettings().radius;
            
            cameraController.LockCamAt(pos, cBodyRadius, fromCreation);
            
            DisableCBodyButtons();
            
            inputCBodyName.text = GetCurrentCBodySettings().cBodyName;
            //todo SetButtonColor(cBodyColorButton, GetCurrentShadingSettings().color);

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
            GetCurrentCBodySettings().cBodyName = cbName;
        }

        public void BeginPickColor()
        {
            // todo coloring
            // OverlayPanel(3,true);
            // colorPicker.CurrentColor = GetCurrentShadingSettings().color;
            // colorPicker.onValueChanged.AddListener(SetCBodyColor);
        }

        private void SetCBodyColor(Color color)
        {
            // todo coloring
            // Shading.ShadingSettings ss = GetCurrentShadingSettings();
            // ss.color = color;
            // SetCurrentShadingSettings(ss);
            //
            // SetButtonColor(cBodyColorButton, color);
        }

        public void EndPickColor()
        {
            colorPicker.onValueChanged.RemoveListener(SetCBodyColor);
            OverlayPanel(3, false);
        }

        private void DestroyCurrentCBody()
        {
            GetCurrentCBodySettings().Unsubscribe();
            Destroy(_cBodyPreviews[_currentCBodyIndex].gameObject);
            _cBodyPreviews.RemoveAt(_currentCBodyIndex);
            _systemSettings.cBodiesSettings.RemoveAt(_currentCBodyIndex);
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
            Physics.PhysicsSettings ps = GetCurrentPhysicsSettings();
            var percent = 
                (ps.initialVelocity.magnitude - ParameterValues.minVelocity) /
                          (ParameterValues.maxVelocity - ParameterValues.minVelocity);
            _cBodyPreviews[_currentCBodyIndex].velocityArrow.SetArrowHeadPosition(
                ps.initialVelocity.normalized,percent);
        }

        private void SetDragHandlePosition()
        {
            _cBodyPreviews[_currentCBodyIndex].positionDrag.ResetDragHandlePosition();
        }

        private void UpdateContextualSliders()
        {
            Physics.PhysicsSettings ps = GetCurrentPhysicsSettings();
            var radius = (ps.radius - ParameterValues.minRadius) /
                    (ParameterValues.maxRadius - ParameterValues.minRadius);
            var gravity = (ps.surfaceGravity - ParameterValues.minGravity) /
                    (ParameterValues.maxGravity - ParameterValues.minGravity);
            var rotationSpeed = (ps.rotationSpeed - ParameterValues.minRotationSpeed) /
                                (ParameterValues.maxRotationSpeed - ParameterValues.minRotationSpeed);
            
            radiusSlider.value = radius;
            radiusSlider.onValueChanged.Invoke(radius);
            gravitySlider.value = gravity;
            gravitySlider.onValueChanged.Invoke(gravity);
            rotationSlider.value = rotationSpeed;
            rotationSlider.onValueChanged.Invoke(rotationSpeed);

        }

        private Shape.ShapeSettings GetCurrentShapeSettings()
        {
            return _systemSettings.cBodiesSettings[_currentCBodyIndex].shape.GetSettings();
        }

        private void SetCurrentShapeSettings(Shape.ShapeSettings ss)
        {
            _systemSettings.cBodiesSettings[_currentCBodyIndex].shape.SetSettings(ss);
        }
        
        private Shading.ShadingSettings GetCurrentShadingSettings()
        {
            return _systemSettings.cBodiesSettings[_currentCBodyIndex].shading.GetSettings();
        }

        private void SetCurrentShadingSettings(Shading.ShadingSettings ss)
        {
            _systemSettings.cBodiesSettings[_currentCBodyIndex].shading.SetSettings(ss);
        }
        
        private Physics.PhysicsSettings GetCurrentPhysicsSettings()
        {
            return _systemSettings.cBodiesSettings[_currentCBodyIndex].physics.GetSettings();
        }

        private void SetCurrentPhysicsSettings(Physics.PhysicsSettings ps)
        {
            _systemSettings.cBodiesSettings[_currentCBodyIndex].physics.SetSettings(ps);
        }

        private CBodySettings GetCurrentCBodySettings()
        {
            return _systemSettings.cBodiesSettings[_currentCBodyIndex];
        }

        private void OnDestroy()
        {
            if (cameraController.cam)
            {
                _systemSettings.lastCameraPosition = cameraController.cam.transform.position; 
                _systemSettings.lastCameraZoom = cameraController.cam.orthographicSize;
            }
            else
            {
                _systemSettings.lastCameraPosition = Vector3.zero;
                _systemSettings.lastCameraZoom = (cameraController.maxZoom - cameraController.minZoom)/2;
            }
            
            SystemUtils.Instance.SaveSystem(_systemSettings);
        }
    }
}
