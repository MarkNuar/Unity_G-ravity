using System;
using System.Collections.Generic;
using CBodies;
using CBodies.Settings;
using CBodies.Settings.PostProcessing.Atmosphere;
using CBodies.Settings.PostProcessing.Ocean;
using CBodies.Settings.PostProcessing.Ring;
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
        //[SerializeField] private ColorPicker colorPicker = null;
        // [SerializeField] private Button bRandomizeColor = null;
        // [SerializeField] private Button bRandomizeShape = null;
        // [SerializeField] private Button bRandomizeAll = null;
        [SerializeField] private Button bResetRandomization = null;

        [SerializeField] private RectTransform colorHandle = null;
        
        [SerializeField] private CameraController cameraController;


        private Shading.ShadingSettings _shadingS = null;
        private Shape.ShapeSettings _shapeS = null;
        private Physics.PhysicsSettings _physicsS = null;
        private Ocean.OceanSettings _oceanS = null;
        private Atmosphere.AtmosphereSettings _atmosphereS = null;
        private Ring.RingSettings _ringS = null;
        
        
        private void Start()
        {
            // load saved CBodies
            
            var (systemToLoad, isNew) = GameManager.Instance.GetSystemToLoad();
            
            // todo: REMOVE FROM BUILD
            // systemToLoad is null if game started from editing menu, otherwise always not null
            if (systemToLoad == null)
            {
                systemToLoad = "Varudia";
                isNew = true;
            }
            
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
            // Set current system settings, available to whole program
            SystemUtils.Instance.currentSystemSettings = _systemSettings;

            // sometimes the color handle is spawned in the wrong position
            colorHandle.anchoredPosition = new Vector2(0, 0);

            bResetRandomization.interactable = false;
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
            
            FetchCurrentSettings();
            
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

        // This method sets the current cBody index
        private void OpenContextualMenu(int currentCBodyIndex)
        {
            // avoid unwanted clicks
            if (cameraController.isDragging) return;
            
            _currentCBodyIndex = currentCBodyIndex;
            
            FetchCurrentSettings();
            
            SelectCurrentCBody();

            SetDragHandlePosition();
            SetArrowHeadPosition();

            // set gravity and radius
            UpdateContextualSliders();
            
            ShowPanel(1);
        }

        private void FetchCurrentSettings()
        {
            _shapeS = GetCurrentCBodySettings().shape.GetSettings();
            _shadingS = GetCurrentCBodySettings().shading.GetSettings();
            _physicsS = GetCurrentCBodySettings().physics.GetSettings();
            _oceanS = GetCurrentCBodySettings().ocean.GetSettings();
            _atmosphereS = GetCurrentCBodySettings().atmosphere.GetSettings();
            _ringS = GetCurrentCBodySettings().ring.GetSettings();
        }

        private void SetCurrentSettings(CBodyGenerator.UpdateType updateType)
        {
            CBodySettings cb = GetCurrentCBodySettings();
            switch(updateType)
            {
                case CBodyGenerator.UpdateType.Shape:
                    cb.ring.SetSettings(_ringS);
                    cb.shape.SetSettings(_shapeS);
                    break;
                case CBodyGenerator.UpdateType.Shading:
                    cb.ocean.SetSettings(_oceanS);
                    cb.ring.SetSettings(_ringS);
                    cb.atmosphere.SetSettings(_atmosphereS);
                    cb.shading.SetSettings(_shadingS);
                    break;
                case CBodyGenerator.UpdateType.Physics:
                    cb.physics.SetSettings(_physicsS);
                    break;
                case CBodyGenerator.UpdateType.All:
                    cb.ocean.SetSettings(_oceanS);
                    cb.ring.SetSettings(_ringS);
                    cb.atmosphere.SetSettings(_atmosphereS);
                    cb.shape.SetSettings(_shapeS);
                    cb.shading.SetSettings(_shadingS);
                    // cb.physics.SetSettings(_physicsS);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(updateType), updateType, null);
            }
        }

        private void UpdateInitialVelocity(Vector3 d, float p)
        {
            _physicsS.initialVelocity =
                d * ((_physicsS.maxSpeed - _physicsS.minSpeed) * p + _physicsS.minSpeed);
            SetCurrentSettings(CBodyGenerator.UpdateType.Physics);
        }

        private void UpdateInitialPosition(Vector3 p)
        {
            _physicsS.initialPosition = p;
            SetCurrentSettings(CBodyGenerator.UpdateType.Physics);
        }
        
        public void SetCBodyRadius()
        {
            _physicsS.radius = radiusSlider.value * (_physicsS.maxRadius - _physicsS.minRadius) 
                               + _physicsS.minRadius;
            SetCurrentSettings(CBodyGenerator.UpdateType.Physics);
        }

        public void SetCBodyGravity()
        {
            _physicsS.surfaceGravity = gravitySlider.value * (_physicsS.maxSurfaceGravity - _physicsS.minSurfaceGravity) + 
                                       _physicsS.minSurfaceGravity;
            SetCurrentSettings(CBodyGenerator.UpdateType.Physics);
        }

        public void SetCBodyRotation()
        {
            _physicsS.rotationSpeed = rotationSlider.value * (_physicsS.maxRotationSpeed - _physicsS.minRotationSpeed) 
                                      + _physicsS.minRotationSpeed;
            SetCurrentSettings(CBodyGenerator.UpdateType.Physics);
        }
        
        public void OpenEditMenu(bool fromCreation)
        {
            Vector3 pos = _cBodyPreviews[_currentCBodyIndex].cBody.transform.position;

            cameraController.LockCamAt(pos, _physicsS.radius, fromCreation);
            
            DisableCBodyButtons();
            
            inputCBodyName.text = GetCurrentCBodySettings().cBodyName;

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

        public void RandomizeColor()
        {
            bResetRandomization.interactable = true;
            _oceanS.RandomizeShading(true);
            _atmosphereS.RandomizeShading(true);
            _ringS.RandomizeShading(true);
            _shadingS.RandomizeShading(true);
            SetCurrentSettings(CBodyGenerator.UpdateType.Shading);
        }

        public void RandomizeShape()
        {
            bResetRandomization.interactable = true;
            _ringS.RandomizeShape(true);
            _shapeS.RandomizeShape(true);
            SetCurrentSettings(CBodyGenerator.UpdateType.Shape);
        }

        public void RandomizeColorAndShape()
        {
            bResetRandomization.interactable = true;
            _oceanS.RandomizeShading(true);
            _oceanS.RandomizeShape(true);
            _atmosphereS.RandomizeShading(true);
            _ringS.RandomizeShading(true);
            _ringS.RandomizeShape(true);
            _shadingS.RandomizeShading(true);
            _shapeS.RandomizeShape(true);
            SetCurrentSettings(CBodyGenerator.UpdateType.All);
        }

        public void ResetRandomization()
        {
            bResetRandomization.interactable = false;
            _oceanS.RandomizeShading(false);
            _oceanS.RandomizeShape(false);
            _atmosphereS.RandomizeShading(false);
            _ringS.RandomizeShading(false);
            _ringS.RandomizeShape(false);
            _shadingS.RandomizeShading(false);
            _shapeS.RandomizeShape(false);
            SetCurrentSettings(CBodyGenerator.UpdateType.All);
        }
        
        // public void BeginPickColor()
        // {
        //     // OverlayPanel(3,true);
        //     // colorPicker.CurrentColor = GetCurrentShadingSettings().color;
        //     // colorPicker.onValueChanged.AddListener(SetCBodyColor);
        // }
        //
        // private void SetCBodyColor(Color color)
        // {
        //     // Shading.ShadingSettings ss = GetCurrentShadingSettings();
        //     // ss.color = color;
        //     // SetCurrentShadingSettings(ss);
        //     //
        //     // SetButtonColor(cBodyColorButton, color);
        // }
        //
        // public void EndPickColor()
        // {
        //     colorPicker.onValueChanged.RemoveListener(SetCBodyColor);
        //     OverlayPanel(3, false);
        // }

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
        
        // private static void SetButtonColor(Button button, Color color)
        // {
        //     ColorBlock buttonColors = button.colors;
        //     buttonColors.normalColor = color;
        //     button.colors = buttonColors;
        // }

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
                (_physicsS.initialVelocity.magnitude - _physicsS.minSpeed) /
                          (_physicsS.maxSpeed - _physicsS.minSpeed);
            _cBodyPreviews[_currentCBodyIndex].velocityArrow.SetArrowHeadPosition(
                _physicsS.initialVelocity.normalized,percent);
        }

        private void SetDragHandlePosition()
        {
            _cBodyPreviews[_currentCBodyIndex].positionDrag.ResetDragHandlePosition();
        }

        private void UpdateContextualSliders()
        {
            var radius = (_physicsS.radius - _physicsS.minRadius) /
                         (_physicsS.maxRadius - _physicsS.minRadius);
            var gravity = (_physicsS.surfaceGravity - _physicsS.minSurfaceGravity) /
                    (_physicsS.maxSurfaceGravity - _physicsS.minSurfaceGravity);
            var rotationSpeed = (_physicsS.rotationSpeed - _physicsS.minRotationSpeed) /
                                (_physicsS.maxRotationSpeed - _physicsS.minRotationSpeed);
            
            radiusSlider.value = radius;
            radiusSlider.onValueChanged.Invoke(radius);
            gravitySlider.value = gravity;
            gravitySlider.onValueChanged.Invoke(gravity);
            rotationSlider.value = rotationSpeed;
            rotationSlider.onValueChanged.Invoke(rotationSpeed);
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
