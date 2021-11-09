using System;
using CBodies.Data;
using UI.Menu.SystemEditing;
using UnityEngine;

namespace CBodies
{
    //[RequireComponent (typeof (Rigidbody))]
    public class CBody : GravityObject
    {
        private CBodyData _initialCBodyData;
        private Vector3 velocity { get; set; }
        public float mass { get; private set; }
        public Vector3 position => _rb.position;
        public Rigidbody Rigidbody => _rb;
        
        
        private Rigidbody _rb;
        private MeshGenerator _meshGenerator;

        private GameObject _meshObject;
        
        private void Awake () {
            _rb = gameObject.AddComponent<Rigidbody> ();
            _rb.isKinematic = true;
            _meshGenerator = gameObject.AddComponent<MeshGenerator>();
        }

        public void InitializeCBody(CBodyData initialCBodyData, Shader shader)
        {
            _initialCBodyData = initialCBodyData;
            _meshGenerator.material = new Material(shader);
            
            _meshObject = _meshGenerator.GenerateMesh(initialCBodyData.appearance.resolution);

            OnAppearanceUpdate(initialCBodyData.appearance, AppearanceUpdateType.All);
            OnPhysicsUpdate(initialCBodyData.physics);
        }
        
        public void OnAppearanceUpdate(AppearanceData ad, AppearanceUpdateType updateType)
        {
            // big todo
            // todo: pass eventually an enum that tells what to change
            // todo: when resolution changes, rebuild the mesh
            // otherwise change only the color
            switch (updateType)
            {
                case AppearanceUpdateType.Material:
                    RegenerateMaterial(ad);
                    break;
                case AppearanceUpdateType.Mesh:
                    RegenerateMesh(ad);
                    break;
                case AppearanceUpdateType.All:
                    RegenerateMaterial(ad);
                    RegenerateMesh(ad);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(updateType), updateType, null);
            }
            
        }

        private void RegenerateMesh(AppearanceData ad)
        {
            _meshObject = _meshGenerator.GenerateMesh(ad.resolution);
        }

        private void RegenerateMaterial(AppearanceData ad)
        {
            _meshGenerator.material.color = ad.color;
        }

        public void OnPhysicsUpdate(PhysicsData pd)
        {
            // big todo
            Transform tr = transform;
            tr.position = pd.initialPosition;
            tr.localScale = Vector3.one * pd.radius;
            
            mass = pd.surfaceGravity * pd.radius * pd.radius / Constants.GravitationalConstant;
            // _meshObject.transform.localScale = Vector3.one * pd.radius;
        }
        
        public void UpdateVelocity (Vector3 acceleration, float timeStep) {
            velocity += acceleration * timeStep;
        }

        public void UpdatePosition (float timeStep) {
            _rb.MovePosition (_rb.position + velocity * timeStep);
        }
    }
}
