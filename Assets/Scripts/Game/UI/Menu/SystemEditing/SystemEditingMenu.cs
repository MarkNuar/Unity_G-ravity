using System;
using System.Collections.Generic;
using CBodies;
using CBodies.PostProcessing;
using CBodies.Settings;
using CBodies.Settings.PostProcessing.Atmosphere;
using CBodies.Settings.PostProcessing.Ocean;
using CBodies.Settings.PostProcessing.Ring;
using CBodies.Settings.Shading;
using CBodies.Settings.Shape;
using Game.UI.Menu.SystemEditing.Preview;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
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
        
        [SerializeField] private Slider radiusSlider = null;
        [SerializeField] private TMP_Text radiusValue = null;
        [SerializeField] private Slider gravitySlider = null;
        [SerializeField] private TMP_Text gravityValue = null;
        [SerializeField] private Slider speedSlider = null;
        [SerializeField] private TMP_Text speedValue = null;
        [SerializeField] private GameObject speedPanel = null;
        [SerializeField] private GameObject deleteButton = null;
        
        [SerializeField] private TMP_Text txtSystemName = null;
        [SerializeField] private TMP_InputField iBodyName = null;
        
        [SerializeField] private Button bResetRandomization = null;
        [SerializeField] private GameObject HasRealistColorsPanel = null;
        [SerializeField] private GameObject HasOceanPanel = null;
        [SerializeField] private GameObject HasAtmospherePanel = null;
        [SerializeField] private GameObject HasRingPanel = null;

        [SerializeField] private GameObject SimulationSpeedPanel = null;
        [SerializeField] private GameObject SimulationTimeStepPanel = null;

        [SerializeField] private CameraController cameraController;

        
        private Shading.ShadingSettings _shadingS = null;
        private Shape.ShapeSettings _shapeS = null;
        private Physics.PhysicsSettings _physicsS = null;
        private Ocean.OceanSettings _oceanS = null;
        private Atmosphere.AtmosphereSettings _atmosphereS = null;
        private Ring.RingSettings _ringS = null;

        private OrbitDisplay _orbitDisplay;

        public void EnableSimulationControls(bool drawOrbits)
        {
            _orbitDisplay.drawOrbits = drawOrbits;
            if (drawOrbits)
            {
                _orbitDisplay.DrawOrbits();
            }
            else
            {
                _orbitDisplay.HideOrbits();
            }
            
            SimulationSpeedPanel.SetActive(drawOrbits);
            SimulationSpeedPanel.GetComponentInChildren<Slider>().value = _orbitDisplay.simulationSpeed;
            SimulationTimeStepPanel.SetActive(drawOrbits);
            SimulationTimeStepPanel.GetComponentInChildren<Slider>().value = _orbitDisplay.timeStep;
        }

        public void SetSimulationSpeed(float simulationSpeed)
        {
            _orbitDisplay.simulationSpeed = (int)simulationSpeed;
        }

        public void SetSimulationTimeStep(float simulationTimeStep)
        {
            _orbitDisplay.timeStep = simulationTimeStep;
        }
        
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
                    _currentCBodyIndex = _systemSettings.AddNewCBodySettings(CBodySettings.CBodyType.Star);
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
            GameManager.Instance.GetMainCamera().GetComponent<CustomPostProcessing>().AwakeEffects();

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
            OverlayPanel(3, true);
        }

        public void CreateCBodyOfType(string type)
        {
            CBodySettings.CBodyType enumType =
                (CBodySettings.CBodyType) Enum.Parse(typeof(CBodySettings.CBodyType), type);
            _currentCBodyIndex = _systemSettings.AddNewCBodySettings(enumType);
            
            CreateCBodyAndPreview();

            SelectCurrentCBody(); 
            
            FetchCurrentSettings();
            
            OpenAppearanceContextualMenu(true);
        }

        private void CreateCBodyAndPreview()
        {
            GameObject cBodyPreview = Instantiate(cBodyPreviewPrefab);
            CBodyPreview preview = cBodyPreview.GetComponent<CBodyPreview>();
            
            CBodySettings cBodySettings = GetCurrentCBodySettings();
            preview.cBody.cBodyGenerator.cBodySettings = cBodySettings;
            cBodySettings.Subscribe(preview.cBody.cBodyGenerator);
            if(_orbitDisplay) cBodySettings.Subscribe(_orbitDisplay);

            preview.selectButton.onClick.AddListener(() => OpenPhysicsContextualMenu(_systemSettings.cBodiesSettings.IndexOf(cBodySettings)));
            preview.velocityArrow.onDrag.AddListener(UpdateInitialVelocity);
            preview.positionDrag.onDrag.AddListener(UpdateInitialPosition);

            preview.cBodyName.text = cBodySettings.cBodyName;
            preview.cBodyName.color = cBodySettings.shading.GetSettings().mainColor;
            
            CBodyPreviews.Add(preview);
        }

        // This method sets the current cBody index
        private void OpenPhysicsContextualMenu(int currentCBodyIndex)
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

            var isNotStar = GetCurrentCBodySettings().cBodyType != CBodySettings.CBodyType.Star;
            speedPanel.SetActive(isNotStar);
            deleteButton.SetActive(isNotStar);
            
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
            CBodyPreviews[_currentCBodyIndex].cBodyPosition.text = Mathf.Abs(_physicsS.initialPosition.x).ToString("0.0");
            SetCurrentSettings(CBodyGenerator.UpdateType.Physics);
        }
        
        public void SetCBodyRadius()
        {
            _physicsS.radius = radiusSlider.value * (_physicsS.maxRadius - _physicsS.minRadius) 
                               + _physicsS.minRadius;
            radiusValue.text = _physicsS.radius.ToString("0.0");
            SetCurrentSettings(CBodyGenerator.UpdateType.Physics);
        }

        public void SetCBodyGravity()
        {
            _physicsS.surfaceGravity = gravitySlider.value * (_physicsS.maxSurfaceGravity - _physicsS.minSurfaceGravity) + 
                                       _physicsS.minSurfaceGravity;
            gravityValue.text = _physicsS.surfaceGravity.ToString("0.0");
            SetCurrentSettings(CBodyGenerator.UpdateType.Physics);
        }

        public void SetCBodySpeed()
        {
            _physicsS.initialVelocity.y = speedSlider.value * (_physicsS.maxSpeed - _physicsS.minSpeed) + 
                                          _physicsS.minSpeed;
            speedValue.text = _physicsS.initialVelocity.y.ToString("0.0");
            SetCurrentSettings(CBodyGenerator.UpdateType.Physics);
            SetArrowHeadPosition();
        }

        public void OpenAppearanceContextualMenu(bool fromCreation)
        {
            Vector3 pos = CBodyPreviews[_currentCBodyIndex].cBody.transform.position;

            cameraController.LockCamAt(pos, _physicsS.radius, fromCreation);
            
            DisableCBodyButtons();
            
            UpdateEditMenuContent();
            
            HideCurrentCBodySelectionHUD();

            ShowPanel(2);
        }

        private void UpdateEditMenuContent()
        {
            iBodyName.text = GetCurrentCBodySettings().cBodyName;
            // Disable buttons that must not be used
            bResetRandomization.interactable = GetCurrentCBodySettings().IsRandomized();

            var hasRealisticColors = GetCurrentCBodySettings().cBodyType == CBodySettings.CBodyType.Planet ||
                                     GetCurrentCBodySettings().cBodyType == CBodySettings.CBodyType.Moon;
            HasRealistColorsPanel.SetActive(hasRealisticColors);
            if (hasRealisticColors)
                HasAtmospherePanel.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(GetHasRealisticColors());
            
            var hasOcean = GetCurrentCBodySettings().cBodyType == CBodySettings.CBodyType.Planet;
            HasOceanPanel.SetActive(hasOcean);
            if (hasOcean)
                HasOceanPanel.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(_oceanS.hasOcean);
            
            var hasAtmosphere = GetCurrentCBodySettings().cBodyType == CBodySettings.CBodyType.Planet;
            HasAtmospherePanel.SetActive(hasAtmosphere);
            if (hasAtmosphere)
                HasAtmospherePanel.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(_atmosphereS.hasAtmosphere);

            var hasRing = GetCurrentCBodySettings().cBodyType == CBodySettings.CBodyType.Gaseous;
            HasRingPanel.SetActive(hasRing);
            if (hasRing)
                HasRingPanel.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(_ringS.hasRing);

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
            SetCurrentSettings(CBodyGenerator.UpdateType.Shading);
        }
        
        public void HasAtmosphere(bool val)
        {
            _atmosphereS.hasAtmosphere = val;
            SetCurrentSettings(CBodyGenerator.UpdateType.Shading);
        }
        
        public void HasRing(bool val)
        {
            _ringS.hasRing = val;
            SetCurrentSettings(CBodyGenerator.UpdateType.Shading);
        }

        public void HasRealisticColors(bool val)
        {
            _shadingS.realisticColors = val;
            _oceanS.realisticColors = val;
            _atmosphereS.realisticColors = val;
            SetCurrentSettings(CBodyGenerator.UpdateType.Shading);
        }

        private bool GetHasRealisticColors()
        {
            return _shadingS.realisticColors && _oceanS.realisticColors && _atmosphereS.realisticColors;
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
            CBodyPreviews[_currentCBodyIndex].cBodyPosition.text = Mathf.Abs(_physicsS.initialPosition.x).ToString("0.0");
        }

        private void UpdateContextualSliders()
        {
            radiusValue.text = _physicsS.radius.ToString("0.0");
            gravityValue.text = _physicsS.surfaceGravity.ToString("0.0");
            speedValue.text = _physicsS.initialVelocity.y.ToString("0.0");
            
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
