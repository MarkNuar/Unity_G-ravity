using System;
using CBodies.Data;
using UI.Menu.SystemEditing;
using UnityEngine;

namespace CBodies
{
    //[RequireComponent (typeof (Rigidbody))]
    // Observer of SystemData
    public class CBody : GravityObject
    {
        private Vector3 velocity { get; set; }
        public float mass { get; private set; }
        public Vector3 position => _rb.position;
        public Rigidbody Rigidbody => _rb;

        public Shader shader;
        
        private Rigidbody _rb;
        

        private GameObject _meshObject;
        
        
        private MeshGenerator _meshGenerator;
        private MaterialGenerator _materialGenerator;
        
        private void Awake () {
            _rb = gameObject.AddComponent<Rigidbody> ();
            _rb.isKinematic = true;
            
            _meshGenerator = new MeshGenerator
            {
                material = new Material(shader)
            };
            _materialGenerator = new MaterialGenerator();
            
        }

        public void InitializeCBody(CBodyData currentCBodyData)
        {
            
            
            OnAppearanceUpdate(currentCBodyData.appearance, AppearanceUpdateType.All);
            OnPhysicsUpdate(currentCBodyData.physics);
        }
        
        public void OnAppearanceUpdate(AppearanceData ad, AppearanceUpdateType updateType)
        {
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

        public void OnPhysicsUpdate(PhysicsData pd)
        {
            // big todo
            Transform tr = transform;
            tr.position = pd.initialPosition;
            tr.localScale = Vector3.one * pd.radius;
            
            mass = pd.surfaceGravity * pd.radius * pd.radius / Constants.GravitationalConstant;
            // _meshObject.transform.localScale = Vector3.one * pd.radius;
        }
        
        private void RegenerateMesh(AppearanceData ad)
        {
            _meshObject = _meshGenerator.GenerateMesh(ad.resolution, transform, gameObject);
        }

        private void RegenerateMaterial(AppearanceData ad)
        {
            Debug.Log("Edited color");
            _meshGenerator.material.color = ad.color;
        }

        
        public void UpdateVelocity (Vector3 acceleration, float timeStep) {
            velocity += acceleration * timeStep;
        }

        public void UpdatePosition (float timeStep) {
            _rb.MovePosition (_rb.position + velocity * timeStep);
        }
    }
}
