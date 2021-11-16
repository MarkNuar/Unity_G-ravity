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

        private CBodyData _cBodyData;
        
        private void Awake () {
            _rb = gameObject.AddComponent<Rigidbody> ();
            _rb.isKinematic = true;
            
            _meshGenerator = new MeshGenerator
            {
                Material = new Material(shader)
            };
            _materialGenerator = new MaterialGenerator();
        }

        public void GenerateCBody(CBodyData cbd)
        {
            _cBodyData = cbd;
            InitializeCBody();
            GenerateMesh();
            GenerateMaterial();
            GeneratePhysics();
        }

        private void InitializeCBody()
        {
            _meshGenerator.UpdateData(_cBodyData);
            _materialGenerator.UpdateData(_cBodyData);
        }

        public void OnMeshUpdate()
        {
            InitializeCBody();
            GenerateMesh();
        }

        public void OnMaterialUpdate()
        {
            InitializeCBody();
            GenerateMaterial();
        }
        
        public void OnPhysicsUpdate()
        {
            InitializeCBody();
            GeneratePhysics();
        }
        
        private void GenerateMesh()
        {
            // TODO
            MeshData md = _cBodyData.meshData;
            _meshObject = _meshGenerator.GenerateMesh(md.resolution, transform, gameObject);
        }

        private void GenerateMaterial()
        {
            // TODO
            MaterialData md = _cBodyData.materialData;
            _meshGenerator.Material.color = md.color;
        }

        private void GeneratePhysics()
        {
            // TODO
            PhysicsData pd = _cBodyData.physicsData;
            Transform tr = transform;
            tr.position = pd.initialPosition;
            tr.localScale = Vector3.one * pd.radius;

            mass = pd.surfaceGravity * pd.radius * pd.radius / Constants.GravitationalConstant;
        }
        
        public void UpdateVelocity (Vector3 acceleration, float timeStep) {
            velocity += acceleration * timeStep;
        }

        public void UpdatePosition (float timeStep) {
            _rb.MovePosition (_rb.position + velocity * timeStep);
        }
    }
}
