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
        public readonly List<CBodyPreview> CBodyPreviews = new List<CBodyPreview>();
        
        // Index of the currently selected cbody
        private int _currentCBodyIndex = -1;

        public int maxCBodyElements = 6;
        
        // PHYSICS
        [SerializeField] private Slider radiusSlider = null;
        [SerializeField] private Slider gravitySlider = null;
        [SerializeField] private Slider speedSlider = null;
        
        // SHAPE & SHADING
        [SerializeField] private TMP_Text txtSystemName = null;
        [SerializeField] private TMP_InputField iBodyName = null;
        [SerializeField] private Button bResetRandomization = null;
        
        [SerializeField] private Toggle tRealisticColors = null;
        [SerializeField] private Toggle tHasOcean = null;
        
        [SerializeField] private RectTransform colorHandle = null;
        
        [SerializeField] private CameraController cameraController;


        private Shading.ShadingSettings _shadingS = null;
        private Shape.ShapeSettings _shapeS = null;
        private Physics.PhysicsSettings _physicsS = null;
        private Ocean.OceanSettings _oceanS = null;
        private Atmosphere.AtmosphereSettings _atmosphereS = null;
        private Ring.RingSettings _ringS = null;

        private OrbitDisplay _orbitDisplay;
        
        private void Start()
        {
            // load saved CBodies
            var (systemToLoad, isNew) = GameManager.Instance.GetSystemToLoad();
            // Find orbit display, if any
            _orbitDisplay = FindObjectOfType<OrbitDisplay>();
            if (_orbitDisplay) _orbitDisplay.systemEditingMenu = this;
            
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
                    _currentCBodyIndex = _systemSettings.AddNewCBody(CBodySettings.CBodyType.Star);
                    CreateCBodyAndPreview();
                }
                else
                {
                    Debug.Log("Loading " + systemToLoad);
                    SystemSettings systemSettings = SystemUtils.Instance.LoadSystem(systemToLoad);
                    if (systemSettings != null)
                    {
                        _systemSettings = systemSettings;
                        txtSystemName.text = _systemSettings.systemName;
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
            
            
            CBody sun = CBodyPreviews[0].cBody;
            Light l = sun.gameObject.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = Color.white;
            l.range = 40000;
            l.shadows = LightShadows.Soft;
            SunShadowCaster ssc = sun.gameObject.AddComponent<SunShadowCaster>();
            ssc.cameraToTrack = GameManager.Instance.GetMainCamera().transform;
            ssc.trackCamera = false;
            
            _orbitDisplay = FindObjectOfType<OrbitDisplay>();
            if (!_orbitDisplay) return;
            _orbitDisplay.centralBody = CBodyPreviews[0].cBody;
            _orbitDisplay.relativeToBody = true;
        }

        private void SetSystemName(string sysName)
        {
            _systemSettings.systemName = sysName;
            txtSystemName.text = sysName;
        }

        public void BeginCBodyCreation(bool loaded)
        {
            if (CBodyPreviews.Count < maxCBodyElements)
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
            preview.cBody.cBodyGenerator.cBodySettings = cBodySettings;
            cBodySettings.Subscribe(preview.cBody.cBodyGenerator);
            if(_orbitDisplay) cBodySettings.Subscribe(_orbitDisplay);

            preview.selectButton.onClick.AddListener(() => OpenContextualMenu(_systemSettings.cBodiesSettings.IndexOf(cBodySettings)));
            preview.velocityArrow.onDrag.AddListener(UpdateInitialVelocity);
            preview.positionDrag.onDrag.AddListener(UpdateInitialPosition);

            preview.cBodyName.text = cBodySettings.cBodyName;
            preview.cBodyName.color = cBodySettings.shading.GetSettings().mainColor;
            
            CBodyPreviews.Add(preview);
        }

        // This method sets the current cBody index
        private void OpenContextualMenu(int currentCBodyIndex)
        {
            // avoid unwanted clicks
            if (cameraController.isDragging) return;
            
            _currentCBodyIndex = currentCBodyIndex;
            
            FetchCurrentSettings();
            
            SelectCurrentCBody();

            // Set cBody HUD position
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
                    cb.atmosphere.SetSettings(_atmosphereS);
                    cb.ocean.SetSettings(_oceanS);
                    cb.ring.SetSettings(_ringS);
                    cb.shading.SetSettings(_shadingS);
                    break;
                case CBodyGenerator.UpdateType.Physics:
                    cb.physics.SetSettings(_physicsS);
                    break;
                case CBodyGenerator.UpdateType.All:
                    cb.atmosphere.SetSettings(_atmosphereS);
                    cb.ocean.SetSettings(_oceanS);
                    cb.ring.SetSettings(_ringS);
                    cb.shape.SetSettings(_shapeS);
                    cb.shading.SetSettings(_shadingS);
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
            
            var speed = (_physicsS.initialVelocity.y - _physicsS.minSpeed) /
                        (_physicsS.maxSpeed - _physicsS.minSpeed);
            speedSlider.SetValueWithoutNotify(speed);
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

        public void SetCBodySpeed()
        {
            _physicsS.initialVelocity.y = speedSlider.value * (_physicsS.maxSpeed - _physicsS.minSpeed) + 
                                          _physicsS.minSpeed;
            SetCurrentSettings(CBodyGenerator.UpdateType.Physics);
            SetArrowHeadPosition();
        }

        public void OpenEditMenu(bool fromCreation)
        {
            Vector3 pos = CBodyPreviews[_currentCBodyIndex].cBody.transform.position;

            cameraController.LockCamAt(pos, _physicsS.radius, fromCreation);
            
            DisableCBodyButtons();
            
            iBodyName.text = GetCurrentCBodySettings().cBodyName;
            // Disable buttons that must not be used
            bResetRandomization.interactable = GetCurrentCBodySettings().IsRandomized();
            tHasOcean.interactable = GetCurrentCBodySettings().cBodyType == CBodySettings.CBodyType.Planet;
            tRealisticColors.interactable = GetCurrentCBodySettings().cBodyType == CBodySettings.CBodyType.Planet ||
                                            GetCurrentCBodySettings().cBodyType == CBodySettings.CBodyType.Moon;
            
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
            
            if(_orbitDisplay) _orbitDisplay.OnPhysicsUpdate();
            
            _currentCBodyIndex = -1;
            ShowPanel(0);
        }

        public void SetCBodyName(string cbName)
        {
            // store the new data 
            CBodyPreviews[_currentCBodyIndex].cBodyName.text = cbName;
            CBodyPreviews[_currentCBodyIndex].cBodyName.color = GetCurrentCBodySettings().shading.GetSettings().mainColor;
            GetCurrentCBodySettings().cBodyName = cbName;
        }

        public void RandomizeColor()
        {
            _oceanS.RandomizeShading(true);
            _atmosphereS.RandomizeShading(true);
            _ringS.RandomizeShading(true);
            _shadingS.RandomizeShading(true);
            SetCurrentSettings(CBodyGenerator.UpdateType.Shading);
            bResetRandomization.interactable = true;
        }

        public void RandomizeShape()
        {
            _ringS.RandomizeShape(true);
            _shapeS.RandomizeShape(true);
            SetCurrentSettings(CBodyGenerator.UpdateType.Shape);
            bResetRandomization.interactable = true;
        }

        public void RandomizeColorAndShape()
        {
            _oceanS.RandomizeShading(true);
            _oceanS.RandomizeShape(true);
            _atmosphereS.RandomizeShading(true);
            _ringS.RandomizeShading(true);
            _ringS.RandomizeShape(true);
            _shadingS.RandomizeShading(true);
            _shapeS.RandomizeShape(true);
            SetCurrentSettings(CBodyGenerator.UpdateType.All);
            bResetRandomization.interactable = true;
        }

        public void ResetRandomization()
        {
            _oceanS.RandomizeShading(false);
            _oceanS.RandomizeShape(false);
            _atmosphereS.RandomizeShading(false);
            _ringS.RandomizeShading(false);
            _ringS.RandomizeShape(false);
            _shadingS.RandomizeShading(false);
            _shapeS.RandomizeShape(false);
            SetCurrentSettings(CBodyGenerator.UpdateType.All);
            bResetRandomization.interactable = false;
        }

        public void HasOcean(bool val)
        {
            _oceanS.hasOcean = val;
            SetCurrentSettings(CBodyGenerator.UpdateType.All);
        }

        public void HasRealisticColors(bool val)
        {
            _shadingS.realisticColors = val;
            _oceanS.realisticColors = val;
            _atmosphereS.realisticColors = val;
            SetCurrentSettings(CBodyGenerator.UpdateType.All);
        }
        
        private void DestroyCurrentCBody()
        {
            GetCurrentCBodySettings().Unsubscribe();
            Destroy(CBodyPreviews[_currentCBodyIndex].gameObject);
            CBodyPreviews.RemoveAt(_currentCBodyIndex);
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

        private void SelectCurrentCBody()
        {
            for (var i = 0; i < CBodyPreviews.Count; i++)
            {
                if (i == _currentCBodyIndex)
                    CBodyPreviews[i].SelectCBody();
                else
                    CBodyPreviews[i].DeselectCBody();
            }
        }

        private void DeselectCurrentCBody()
        {
            if (_currentCBodyIndex != -1)
            {
                CBodyPreviews[_currentCBodyIndex].DeselectCBody();
                _currentCBodyIndex = -1;
            }
        }

        private void HideCurrentCBodySelectionHUD()
        {
            if (_currentCBodyIndex != -1)
            {
                CBodyPreviews[_currentCBodyIndex].HideSelectionHUD();
            }
        }

        private void DisableCBodyButtons()
        {
            foreach (CBodyPreview help in CBodyPreviews)
            {
                help.selectButton.enabled = false;
            }
        }
        
        private void EnableCBodyButtons()
        {
            foreach (CBodyPreview help in CBodyPreviews)
            {
                help.selectButton.enabled = true;
            }
        }

        private void SetArrowHeadPosition()
        {
            var percent = 
                (_physicsS.initialVelocity.magnitude - _physicsS.minSpeed) /
                          (_physicsS.maxSpeed - _physicsS.minSpeed);
            CBodyPreviews[_currentCBodyIndex].velocityArrow.SetArrowHeadPosition(
                _physicsS.initialVelocity.normalized,percent);
        }

        private void SetDragHandlePosition()
        {
            CBodyPreviews[_currentCBodyIndex].positionDrag.ResetDragHandlePosition();
        }

        private void UpdateContextualSliders()
        {
            var radius = (_physicsS.radius - _physicsS.minRadius) /
                         (_physicsS.maxRadius - _physicsS.minRadius);
            var gravity = (_physicsS.surfaceGravity - _physicsS.minSurfaceGravity) /
                    (_physicsS.maxSurfaceGravity - _physicsS.minSurfaceGravity);
            var speed = (_physicsS.initialVelocity.y - _physicsS.minSpeed) /
                        (_physicsS.maxSpeed - _physicsS.minSpeed);
            
            radiusSlider.SetValueWithoutNotify(radius);
            gravitySlider.SetValueWithoutNotify(gravity);
            speedSlider.SetValueWithoutNotify(speed);
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
