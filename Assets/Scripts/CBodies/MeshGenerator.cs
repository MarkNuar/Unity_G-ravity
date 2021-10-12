using System;
using UnityEngine;

namespace Planets
{
    public class MeshGenerator : MonoBehaviour
    {
        public int terrainResolution = 10; // todo import from general settings 
        public Material material;
        private Mesh _mesh;

        private void Start()
        {
            GenerateMesh();
        }

        public void GenerateMesh()
        {
            if (_mesh == null) {
                _mesh = new Mesh ();
            }
            else {
                _mesh.Clear();
            }

            var s = new IcoSphereGenerator ();
            s.Generate (terrainResolution);
            _mesh.vertices = s.Vertices;
            _mesh.triangles = s.Triangles;
            _mesh.RecalculateBounds ();
            _mesh.RecalculateNormals ();

            var g = GetOrCreateMeshObject ("Mesh", _mesh, material);
            if (!g.GetComponent<MeshCollider> ()) {
                g.AddComponent<MeshCollider> ();
            }
            g.GetComponent<MeshCollider> ().sharedMesh = _mesh;
        }

        private GameObject GetOrCreateMeshObject (string name, Mesh mesh, Material material) {
            // Find/create object
            var child = transform.Find (name);
            if (!child) {
                child = new GameObject (name).transform;
                child.parent = transform;
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
                child.localScale = Vector3.one * 100;
                child.gameObject.layer = gameObject.layer;
            }

            // Add mesh components
            if (!child.TryGetComponent<MeshFilter> (out var filter)) {
                filter = child.gameObject.AddComponent<MeshFilter> ();
            }
            filter.sharedMesh = mesh;

            if (!child.TryGetComponent<MeshRenderer> (out var renderer)) {
                renderer = child.gameObject.AddComponent<MeshRenderer> ();
            }
            renderer.sharedMaterial = material;

            return child.gameObject;
        }
    }
}
