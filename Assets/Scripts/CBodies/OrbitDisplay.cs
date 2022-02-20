using System;
using System.Collections;
using System.Collections.Generic;
using Game.UI.Menu.SystemEditing;
using Game.UI.Menu.SystemEditing.Preview;
using UnityEngine;
using Physics = CBodies.Settings.Physics.Physics;

namespace CBodies
{
    public class OrbitDisplay : MonoBehaviour, ICBodyObserver
    {
        public bool drawOrbits;
        
        public int numSteps = 1000;
        public float timeStep = 0.1f;

        [Tooltip("When simulation speed is set to 1, the number of points computed per second is equal to " +
                 "the framerate of the game")]
        [Range(1, 100)] public int simulationSpeed = 5;
        
        public bool relativeToBody;
        public CBody centralBody;
        public float width = 100;
        public bool useThickLines;

        private List<CBodyPreview> _previews;
        private VirtualBody[] _virtualBodies;
        private Vector3[][] _drawPoints;
        private int _referenceFrameIndex;
        private Vector3 _referenceBodyInitialPosition;

        public SystemEditingMenu systemEditingMenu;
        
        private void Update ()
        {
            if (!Input.GetKeyDown(KeyCode.T)) return;
            drawOrbits = !drawOrbits;
            if (drawOrbits)
            {
                DrawOrbits();
            }
            else
            {
                HideOrbits();
            }
        }

        private IEnumerator BeforeDrawTimer(float time)
        {
            yield return new WaitForSeconds(time);
            DrawOrbits();
        }
        
        private void DrawOrbits () {
            // Hides previous drawn orbits
            HideOrbits();
            
            // _previews = FindObjectsOfType<CBodyPreview> ();
            _previews = systemEditingMenu.CBodyPreviews;
            _virtualBodies = new VirtualBody[_previews.Count];
            _drawPoints = new Vector3[_previews.Count][];
            _referenceFrameIndex = 0;
            _referenceBodyInitialPosition = Vector3.zero;

            // Initialize virtual bodies (don't want to move the actual bodies)
            for (var i = 0; i < _virtualBodies.Length; i++) {
                _virtualBodies[i] = new VirtualBody (_previews[i].cBody);
                _drawPoints[i] = new Vector3[numSteps];

                if (_previews[i].cBody == centralBody && relativeToBody) {
                    _referenceFrameIndex = i;
                    _referenceBodyInitialPosition = _virtualBodies[i].Position;
                }
                
                // setup line renderer
                Color pathColour = _previews[i].cBody.cBodyGenerator.cBodySettings.shading.GetSettings().mainColor;
                LineRenderer lineRenderer = _previews[i].lineRenderer;
                lineRenderer.enabled = true;
                lineRenderer.positionCount = numSteps;
                lineRenderer.startColor = pathColour;
                lineRenderer.endColor = pathColour;
                lineRenderer.widthMultiplier = width;
            }

            StartCoroutine(DrawOrbitsCoroutine());
        }

        IEnumerator DrawOrbitsCoroutine()
        {
            // Simulate
            // int count = 0;
            for (var step = 0; step < numSteps; step++) 
            // while(true)
            {
                
                Vector3 referenceBodyPosition = (relativeToBody) ? _virtualBodies[_referenceFrameIndex].Position : Vector3.zero;
                // Update velocities
                for (var i = 0; i < _virtualBodies.Length; i++) {
                    _virtualBodies[i].Velocity += CalculateAcceleration (i, _virtualBodies) * timeStep;
                }
                // Update positions
                for (var i = 0; i < _virtualBodies.Length; i++) {
                    Vector3 newPos = _virtualBodies[i].Position + _virtualBodies[i].Velocity * timeStep;
                    _virtualBodies[i].Position = newPos;
                    if (relativeToBody) {
                        Vector3 referenceFrameOffset = referenceBodyPosition - _referenceBodyInitialPosition;
                        newPos -= referenceFrameOffset;
                    }
                    if (relativeToBody && i == _referenceFrameIndex) {
                        newPos = _referenceBodyInitialPosition;
                    }
                    // _drawPoints[i][step] = newPos;

                    // if (step < (2 * 3.14 * _previews[i].cBody.transform.position.x))
                    // {
                        _previews[i].lineRenderer.SetPosition(step, newPos);
                        // if (count >= numSteps)
                        // {
                        //     _previews[i].lineRenderer.positionCount = 0;
                        //     _previews[i].lineRenderer.positionCount = numSteps;
                        //     
                        // }
                        // _previews[i].lineRenderer.SetPosition(count % numSteps, newPos);
                    // }


                }

                // count++;
                if(step % simulationSpeed == 0 )
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
                acceleration += forceDir * Constants.GravitationalConstant * virtualBodies[j].Mass / sqrDst;
            }
            return acceleration;
        }

        private void HideOrbits ()
        {
            StopAllCoroutines();
            _previews = systemEditingMenu.CBodyPreviews;
            // Draw paths
            foreach (CBodyPreview p in _previews)
            {
                p.lineRenderer.positionCount = 0;
            }
        }

        private void UpdateColors()
        {
            _previews = systemEditingMenu.CBodyPreviews;
            foreach (CBodyPreview p in _previews)
            {
                if (!p.Selected) continue;
                Color pathColour = p.cBody.cBodyGenerator.cBodySettings.shading.GetSettings().mainColor;
                p.lineRenderer.startColor = pathColour;
                p.lineRenderer.endColor = pathColour;
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
            if (drawOrbits)
            {
                HideOrbits();
                StopAllCoroutines();
                StartCoroutine(BeforeDrawTimer(0.2f));
                //DrawOrbits();
            }
            else
            {
                HideOrbits();
            }
        }

        public void OnShapeUpdate()
        {
            return;
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