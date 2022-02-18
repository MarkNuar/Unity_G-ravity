using System;
using Game.UI.Menu.SystemEditing.Preview;
using UnityEngine;
using Physics = CBodies.Settings.Physics.Physics;

namespace CBodies
{
    public class OrbitDisplay : MonoBehaviour
    {
        public bool drawOrbits;
        
        public int numSteps = 1000;
        public float timeStep = 0.1f;

        public bool relativeToBody;
        public CBody centralBody;
        public float width = 100;
        public bool useThickLines;
        
        private void Update () {
            if (drawOrbits)
            {
                DrawOrbits();
            }
            else
            {
                HideOrbits();
            }

            // if (Input.GetKeyDown(KeyCode.T))
            // {
            //     // show trajectory
            // }
            //
            // if (Input.GetKeyDown(KeyCode.H))
            // {
            //     // hide trajectory
            // }
        }

        private void DrawOrbits () {
            var previews = FindObjectsOfType<CBodyPreview> ();
            var virtualBodies = new VirtualBody[previews.Length];
            var drawPoints = new Vector3[previews.Length][];
            var referenceFrameIndex = 0;
            Vector3 referenceBodyInitialPosition = Vector3.zero;

            // Initialize virtual bodies (don't want to move the actual bodies)
            for (var i = 0; i < virtualBodies.Length; i++) {
                virtualBodies[i] = new VirtualBody (previews[i].cBody);
                drawPoints[i] = new Vector3[numSteps];

                if (previews[i].cBody == centralBody && relativeToBody) {
                    referenceFrameIndex = i;
                    referenceBodyInitialPosition = virtualBodies[i].Position;
                }
            }

            // Simulate
            for (var step = 0; step < numSteps; step++) {
                Vector3 referenceBodyPosition = (relativeToBody) ? virtualBodies[referenceFrameIndex].Position : Vector3.zero;
                // Update velocities
                for (var i = 0; i < virtualBodies.Length; i++) {
                    virtualBodies[i].Velocity += CalculateAcceleration (i, virtualBodies) * timeStep;
                }
                // Update positions
                for (var i = 0; i < virtualBodies.Length; i++) {
                    Vector3 newPos = virtualBodies[i].Position + virtualBodies[i].Velocity * timeStep;
                    virtualBodies[i].Position = newPos;
                    if (relativeToBody) {
                        Vector3 referenceFrameOffset = referenceBodyPosition - referenceBodyInitialPosition;
                        newPos -= referenceFrameOffset;
                    }
                    if (relativeToBody && i == referenceFrameIndex) {
                        newPos = referenceBodyInitialPosition;
                    }
                    drawPoints[i][step] = newPos;
                }
            }

            // Draw paths
            for (var bodyIndex = 0; bodyIndex < virtualBodies.Length; bodyIndex++) {
                // Color pathColour = bodies[bodyIndex].gameObject.GetComponentInChildren<MeshRenderer> ().sharedMaterial.color; //

                Color pathColour = Color.white;

                if (useThickLines) {
                    var lineRenderer = previews[bodyIndex].gameObject.GetComponentInChildren<LineRenderer> ();
                    lineRenderer.enabled = true;
                    lineRenderer.positionCount = drawPoints[bodyIndex].Length;
                    lineRenderer.SetPositions (drawPoints[bodyIndex]);
                    lineRenderer.startColor = pathColour;
                    lineRenderer.endColor = pathColour;
                    lineRenderer.widthMultiplier = width;
                } else {
                    for (var i = 0; i < drawPoints[bodyIndex].Length - 1; i++) {
                        Debug.DrawLine (drawPoints[bodyIndex][i], drawPoints[bodyIndex][i + 1], pathColour);
                    }

                    // Hide renderer
                    var lineRenderer = previews[bodyIndex].gameObject.GetComponentInChildren<LineRenderer> ();
                    if (lineRenderer) {
                        lineRenderer.enabled = false;
                    }
                }
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

        private static void HideOrbits ()
        {
            var previews = FindObjectsOfType<CBodyPreview> ();

            // Draw paths
            foreach (CBodyPreview p in previews)
            {
                var lineRenderer = p.gameObject.GetComponentInChildren<LineRenderer> ();
                lineRenderer.positionCount = 0;
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
    }
}