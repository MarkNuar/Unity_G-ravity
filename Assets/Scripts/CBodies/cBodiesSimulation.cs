using System.Collections;
using System.Collections.Generic;
using Game.UI.Menu.SystemEditing;
using Game.UI.Menu.SystemEditing.Preview;
using UnityEngine;
using Physics = CBodies.Settings.Physics.Physics;

namespace CBodies
{
    public class cBodiesSimulation : MonoBehaviour, ICBodyObserver
    {
        public bool simulating;

        [Tooltip("Increment in the lenght of the line. When 1 the line is as long as the circle with radius the distance of the planet from the centre")]
        [Range(1, 5)]public int lengthIncrement = 2;

        [Tooltip("Time step of the simulation, the smaller, the more accurate the simulation is")]
        [Range(0.001f, 1)]public float timeStep = 0.001f;

        [Tooltip("Number of points computed at each application update")]
        [Range(1, 25)] public int simulationSpeed = 1;
        
        public bool relativeToBody;
        public CBody centralBody;
        public int centralBodyIndex;
        public float width = 100;

        private List<CBodyPreview> _previews;
        private VirtualBody[] _virtualBodies;

        private List<List<Vector3>> _points;
        private List<int> _sizes;
        private int _referenceFrameIndex;
        private Vector3 _referenceBodyInitialPosition;

        public SystemEditingMenu systemEditingMenu;

        public bool showOrbits = true;
        // private void Update ()
        // {
        //     if (!Input.GetKeyDown(KeyCode.F10)) return;
        //     drawOrbits = !drawOrbits;
        //     if (drawOrbits)
        //     {
        //         DrawOrbits();
        //     }
        //     else
        //     {
        //         HideOrbits();
        //     }
        // }

        private IEnumerator BeforeDrawTimer(float time)
        {
            yield return new WaitForSeconds(time);
            StartSimulation();
        }
        
        public void StartSimulation ()
        {
            // Hides previous drawn orbits
            StopSimulation();
            
            systemEditingMenu.DeselectCurrentCBody();
            
            _previews = systemEditingMenu.CBodyPreviews;
            
            foreach(CBodyPreview prev in _previews) prev.ShowEditingHUD(false);
            
            _virtualBodies = new VirtualBody[_previews.Count];
            
            _points = new List<List<Vector3>>(_previews.Count);
            _sizes = new List<int>(_previews.Count);
            _referenceFrameIndex = 0;
            _referenceBodyInitialPosition = Vector3.zero;

            // Initialize virtual bodies (don't want to move the actual bodies)
            for (var i = 0; i < _virtualBodies.Length; i++)
            {
                //var distToCentre = Mathf.Max(
                    //(_previews[i].cBody.cBodyGenerator.cBodySettings.physics.GetSettings().initialPosition -
                     //_previews[0].cBody.cBodyGenerator.cBodySettings.physics.GetSettings().initialPosition).magnitude,
                   // 0.01f);
                _sizes.Add(ComputeSize());
                _points.Add(new List<Vector3>(_sizes[i]));
                
                 
                _virtualBodies[i] = new VirtualBody(_previews[i].cBody);
                if (_previews[i].cBody == centralBody && relativeToBody)
                {
                    _referenceFrameIndex = i;
                    _referenceBodyInitialPosition = _virtualBodies[i].Position;
                }

                // setup line renderer
                Color pathColour = _previews[i].cBody.cBodyGenerator.cBodySettings.shading.GetSettings().mainColor;
                LineRenderer lineRenderer = _previews[i].lineRenderer;
                lineRenderer.enabled = true;
                // lineRenderer.positionCount = _sizes[i];
                lineRenderer.startColor = Color.black;
                lineRenderer.endColor = pathColour;
                lineRenderer.widthMultiplier = width;
            }

            StartCoroutine(StartSimulationCoroutine());
        }

        private int ComputeSize()
        {
            //return Mathf.CeilToInt(Remap((100 / timeStep), 100, 100000, 200, 1000));
            return Mathf.Clamp(Mathf.CeilToInt(400 / timeStep), 0, 4000);
        }
        
        public static float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
        {
            float t = Mathf.InverseLerp(oldLow, oldHigh, input);
            return Mathf.Lerp(newLow, newHigh, t);
        }

        IEnumerator StartSimulationCoroutine()
        {
            // Simulate
            var count = 0;
            // for (var step = 0; step < numSteps; step++) 
            while(true)
            {
                while (GameManager.Instance.gamePaused) yield return null;
                
                Vector3 referenceBodyPosition = (relativeToBody) ? _virtualBodies[_referenceFrameIndex].Position : Vector3.zero;
                // Update velocities
                for (var i = 0; i < _virtualBodies.Length; i++) {
                    _virtualBodies[i].Velocity += CalculateAcceleration (i, _virtualBodies) * timeStep;
                }
                // Update positions
                for (var i = 0; i < _virtualBodies.Length; i++)
                {
                    _sizes[i] = ComputeSize(); // sizes can change during simulation
                    
                    Vector3 newPos = _virtualBodies[i].Position + _virtualBodies[i].Velocity * timeStep;
                    _virtualBodies[i].Position = newPos;
                    if (relativeToBody) {
                        Vector3 referenceFrameOffset = referenceBodyPosition - _referenceBodyInitialPosition;
                        newPos -= referenceFrameOffset;
                    }
                    if (relativeToBody && i == _referenceFrameIndex) {
                        newPos = _referenceBodyInitialPosition;
                    }

                    if (_points[i].Count >= _sizes[i])
                    {
                        _points[i].RemoveRange(0, _points[i].Count - _sizes[i] + 1);
                        //_points[i].Dequeue();
                    }
                    _points[i].Add(newPos);

                    // TODO: MOVE ALSO CBODIES
                    Transform tr = _previews[i].cBody.transform;
                    tr.position = newPos;
                    if(i != _referenceFrameIndex)
                        tr.RotateAround(tr.position, new Vector3(0,0,1), 
                        _previews[i].cBody.cBodyGenerator.cBodySettings.physics.GetSettings().initialRotationSpeed * timeStep);
                    _previews[i].cBody.cBodyGenerator.cBodySettings.ring.InitSettings();
                    
                    if (showOrbits)
                    {
                        _previews[i].lineRenderer.positionCount = _points[i].Count;// + 1;
                        _previews[i].lineRenderer.SetPositions(_points[i].ToArray());
                        //_previews[i].lineRenderer.SetPosition(_points[i].Count, _referenceBodyInitialPosition);
                    }
                    else
                    {
                        _previews[i].lineRenderer.positionCount = 0;
                    }
                }

                count++;
                if (count % simulationSpeed != 0) continue;
                count = 0;
                yield return null;
            }
        }

        private static Vector3 CalculateAcceleration (int i, VirtualBody[] virtualBodies) {
            Vector3 acceleration = Vector3.zero;
            for (var j = 0; j < virtualBodies.Length; j++) {
                if (i == j) {
                    continue;
                }
                Vector3 forceDir = (virtualBodies[j].Position - virtualBodies[i].Position).normalized;
                float sqrDst = (virtualBodies[j].Position - virtualBodies[i].Position).sqrMagnitude;
                acceleration += forceDir * (Constants.GravitationalConstant * virtualBodies[j].Mass) / sqrDst;
            }
            return acceleration;
        }

        public void StopSimulation ()
        {
            StopAllCoroutines();
            _previews = systemEditingMenu.CBodyPreviews;
            // Draw paths
            foreach (CBodyPreview p in _previews)
            {
                p.cBody.transform.position = p.cBody.cBodyGenerator.cBodySettings.physics.GetSettings().initialPosition;
                p.cBody.transform.rotation = Quaternion.Euler(0,0,0);
                p.lineRenderer.positionCount = 0;
                p.cBody.cBodyGenerator.cBodySettings.ring.InitSettings();
                p.ShowEditingHUD(true);
            }
        }
        
        public void ToggleOrbits(bool show)
        {
            showOrbits = show;
            // StopAllCoroutines();
            // _previews = systemEditingMenu.CBodyPreviews;
            // // Draw paths
            // foreach (CBodyPreview p in _previews)
            // {
            //     p.cBody.transform.position = p.cBody.cBodyGenerator.cBodySettings.physics.GetSettings().initialPosition;
            //     p.lineRenderer.positionCount = 0;
            // }
        }

        private void UpdateColors()
        {
            _previews = systemEditingMenu.CBodyPreviews;
            foreach (CBodyPreview p in _previews)
            {
                if (!p.Selected) continue;
                Color pathColour = p.cBody.cBodyGenerator.cBodySettings.shading.GetSettings().mainColor;
                p.lineRenderer.startColor = Color.black;
                p.lineRenderer.endColor = pathColour;
                // update color of cBody name too
                systemEditingMenu.SetCBodyName(p.cBody.cBodyGenerator.cBodySettings.cBodyName);
            }
        }
        
        private class VirtualBody {
            public Vector3 Position;
            public Vector3 Velocity;
            public readonly float Mass;

            public VirtualBody (CBody body)
            {
                Physics.PhysicsSettings ps = body.cBodyGenerator.cBodySettings.physics.GetSettings();
                Position = ps.initialPosition;
                Velocity = ps.initialVelocity;
                Mass = body.mass;
            }
        }
        
        public void OnPhysicsUpdate()
        {
            if (simulating)
            {
                // _waitingForOrbits = true;
                StopSimulation();
                StopAllCoroutines();
                StartCoroutine(BeforeDrawTimer(0.2f));
            }
        }

        public void OnShapeUpdate()
        {
        }

        public void OnShadingUpdate()
        {
            UpdateColors();
        }

        public void OnInitialUpdate()
        {
            OnPhysicsUpdate();
        }
    }
}