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
using UnityEditor;
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
        [SerializeField] private Slider rotationSlider = null;
        [SerializeField] private TMP_Text rotationValue = null;

        [SerializeField] private TMP_Text txtSystemName = null;
        [SerializeField] private TMP_InputField iBodyName = null;
        
        [SerializeField] private Button bResetRandomization = null;
        [SerializeField] private GameObject HasRealistColorsPanel = null;
        [SerializeField] private GameObject HasOceanPanel = null;
        [SerializeField] private GameObject HasAtmospherePanel = null;
        [SerializeField] private GameObject HasRingPanel = null;

        [SerializeField] private GameObject SimulationSpeedPanel = null;
        [SerializeField] private GameObject SimulationTimeStepPanel = null;
        [SerializeField] private GameObject ToggleOrbitsPanel = null;

        [SerializeField] private CameraController cameraController;

        [SerializeField] private TMP_Text errorMessage;

        [SerializeField] private Toggle trackCBody;

        private Shading.ShadingSettings _shadingS = null;
        private Shape.ShapeSettings _shapeS = null;
        private Physics.PhysicsSettings _physicsS = null;
        private Ocean.OceanSettings _oceanS = null;
        private Atmosphere.AtmosphereSettings _atmosphereS = null;
        private Ring.RingSettings _ringS = null;

        private cBodiesSimulation _cBodiesSimulation;

        private FakePlayMode _fakePlayMode;
        
        public void ToggleCBodyTracking(bool tracking)
        {
            ToggleCBodyTracking(tracking, _currentCBodyIndex);
        }

        private void ToggleCBodyTracking(bool tracking, int index)
        {
            if (tracking)
            {
                _fakePlayMode.trackCBody = true;
                _fakePlayMode.transformToTrack = CBodyPreviews[index].cBody.transform;
                _fakePlayMode.trackedCBodyIndex = index;

                _cBodiesSimulation.relativeToBody = true;
                _cBodiesSimulation.centralBody = CBodyPreviews[index].cBody;
                _cBodiesSimulation.centralBodyIndex = index;
            }
            else
            {
                _fakePlayMode.trackCBody = false;
                _fakePlayMode.transformToTrack = null;
                _fakePlayMode.trackedCBodyIndex = -1;

                _cBodiesSimulation.relativeToBody = false;
                _cBodiesSimulation.centralBody = null;
                _cBodiesSimulation.centralBodyIndex = -1;
            }
        }
        
        
        public void ToggleSimulation(bool toggleSimulation)
        {
            _cBodiesSimulation.simulating = toggleSimulation;
            if (toggleSimulation)
            {
                ShowPanel(4);
                _cBodiesSimulation.StartSimulation();
            }
            else
            {
                ShowPanel(0);
                OverlayPanel(4, true);
                _cBodiesSimulation.StopSimulation();
            }
            
            SimulationSpeedPanel.SetActive(toggleSimulation);
            SimulationSpeedPanel.GetComponentInChildren<Slider>().value = _cBodiesSimulation.simulationSpeed;
            SimulationTimeStepPanel.SetActive(toggleSimulation);
            SimulationTimeStepPanel.GetComponentInChildren<Slider>().value = _cBodiesSimulation.timeStep;
            ToggleOrbitsPanel.SetActive(toggleSimulation);
            ToggleOrbitsPanel.GetComponentInChildren<Toggle>().isOn = _cBodiesSimulation.showOrbits;
        }

        public void ShowOrbits(bool show)
        {
            _cBodiesSimulation.showOrbits = show;
        }

        public void SetSimulationSpeed(float simulationSpeed)
        {
            _cBodiesSimulation.simulationSpeed = (int)simulationSpeed;
        }

        public void SetSimulationTimeStep(float simulationTimeStep)
        {
            _cBodiesSimulation.timeStep = simulationTimeStep;
        }
        
        private void Start()
        {
            // load saved CBodies
            var (systemToLoad, isNew) = GameManager.Instance.GetSystemToLoad();
            // Find orbit display, if any
            _cBodiesSimulation = FindObjectOfType<cBodiesSimulation>();
            if (_cBodiesSimulation) _cBodiesSimulation.systemEditingMenu = this;

            _fakePlayMode = FindObjectOfType<FakePlayMode>();
            
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
            
            
            if (!_cBodiesSimulation) return;
            ToggleCBodyTracking(true, 0); // set central sun as reference cBody for orbits and exploration 
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
                var error = "Cannot add more than " + maxCBodyElements + " cBodies";
                errorMessage.text = error;
                OverlayPanel(5, true);
            }
        }

        public void CloseErrorMessage()
        {
            OverlayPanel(5, false);
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

            FetchCurrentSettings();
            
            SelectCurrentCBody(); 
            
            if(!_cBodiesSimulation.simulating)
                OpenAppearanceContextualMenu(true);
        }

        private void CreateCBodyAndPreview()
        {
            GameObject cBodyPreview = Instantiate(cBodyPreviewPrefab);
            CBodyPreview preview = cBodyPreview.GetComponent<CBodyPreview>();
            
            CBodySettings cBodySettings = GetCurrentCBodySettings();
            preview.cBody.cBodyGenerator.cBodySettings = cBodySettings;
            cBodySettings.Subscribe(preview.cBody.cBodyGenerator);
            if(_cBodiesSimulation) cBodySettings.Subscribe(_cBodiesSimulation);

            preview.selectButton.onClick.AddListener(() => OpenPhysicsContextualMenu(_systemSettings.cBodiesSettings.IndexOf(cBodySettings)));
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
            if (_cBodiesSimulation.simulating) return;
            
            _currentCBodyIndex = currentCBodyIndex;
            
            FetchCurrentSettings();
            
            SelectCurrentCBody();

            // Set cBody HUD position
            SetDragHandlePosition();

            // set gravity and radius
            UpdateContextualSliders();
            
            // set tracking toggle 
            trackCBody.SetIsOnWithoutNotify(
                _fakePlayMode.trackCBody && 
                _fakePlayMode.trackedCBodyIndex == _currentCBodyIndex && 
                _cBodiesSimulation.relativeToBody && 
                _cBodiesSimulation.centralBodyIndex == _currentCBodyIndex);

            // var isNotStar = GetCurrentCBodySettings().cBodyType != CBodySettings.CBodyType.Star;
            // speedPanel.SetActive(isNotStar);
            // rotationPanel.SetActive(isNotStar);
            // deleteButton.SetActive(isNotStar);
            
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
        }

        public void SetCBodyRotation()
        {
            _physicsS.initialRotationSpeed = rotationSlider.value * (_physicsS.maxRotationSpeed - _physicsS.minRotationSpeed) + 
                                          _physicsS.minRotationSpeed;
            rotationValue.text = _physicsS.initialRotationSpeed.ToString("0.0");
            SetCurrentSettings(CBodyGenerator.UpdateType.Physics);
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
            if(_cBodiesSimulation.simulating) return;
            
            cameraController.FreeCam();

            EnableCBodyButtons();
            DeselectCurrentCBody();

            ShowPanel(0);
            // if (_cBodiesSimulation.simulating)
            // {
            //     ToggleSimulationPanel.GetComponentInChildren<Toggle>().isOn = false;
            //     ToggleSimulation(false);
            // }
            OverlayPanel(4, true);
        }

        public void DeleteCBody()
        {
            DestroyCurrentCBody();
            
            if(_cBodiesSimulation) _cBodiesSimulation.OnPhysicsUpdate();

            if (_fakePlayMode.trackCBody && 
                _fakePlayMode.trackedCBodyIndex == _currentCBodyIndex && 
                _cBodiesSimulation.relativeToBody && 
                _cBodiesSimulation.centralBodyIndex == _currentCBodyIndex)
            {
                ToggleCBodyTracking(false);
            }
            
            _currentCBodyIndex = -1;
            ShowPanel(0);
            OverlayPanel(4, true);
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
            if (_cBodiesSimulation.simulating)
            {
                DeselectCurrentCBody();
                CloseAllMenu();
                return;
            }
            for (var i = 0; i < CBodyPreviews.Count; i++)
            {
                if (i == _currentCBodyIndex)
                    CBodyPreviews[i].SelectCBody();
                else
                    CBodyPreviews[i].DeselectCBody();
            }
        }

        public void DeselectCurrentCBody()
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
                CBodyPreviews[_currentCBodyIndex].ToggleSelectionHUD(false);
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
            rotationValue.text = _physicsS.initialRotationSpeed.ToString("0.0");
            
            var radius = (_physicsS.radius - _physicsS.minRadius) /
                         (_physicsS.maxRadius - _physicsS.minRadius);
            var gravity = (_physicsS.surfaceGravity - _physicsS.minSurfaceGravity) /
                    (_physicsS.maxSurfaceGravity - _physicsS.minSurfaceGravity);
            var speed = (_physicsS.initialVelocity.y - _physicsS.minSpeed) /
                        (_physicsS.maxSpeed - _physicsS.minSpeed);
            var rotation= (_physicsS.initialRotationSpeed - _physicsS.minRotationSpeed) /
                       (_physicsS.maxRotationSpeed - _physicsS.minRotationSpeed);
            
            radiusSlider.SetValueWithoutNotify(radius);
            gravitySlider.SetValueWithoutNotify(gravity);
            speedSlider.SetValueWithoutNotify(speed);
            rotationSlider.SetValueWithoutNotify(rotation);
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
