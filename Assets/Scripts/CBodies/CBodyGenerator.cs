using System;
using System.Collections.Generic;
using CBodies.Settings;
using CBodies.Settings.Shape;
using UnityEngine;
using Utilities;
using Physics = CBodies.Settings.Physics.Physics;

namespace CBodies
{
    public class CBodyGenerator : MonoBehaviour
    {
        public CBody cBody;
        
        public enum PreviewMode { LOD0, LOD1, LOD2, CollisionRes }
        public ResolutionSettings resolutionSettings = new ResolutionSettings();
        public PreviewMode previewMode = PreviewMode.LOD0;
        
        // CBodySettings
        public CBodySettings cBodySettings;
        
        // Private variables
        private Mesh _previewMesh;
        private Mesh _collisionMesh;
        private Mesh[] _lodMeshes;
        
        // Buffer for storing vertices sent to the GPU 
        private ComputeBuffer _vertexBuffer;
        
        private bool _shapeUpdated;
        private bool _shadingUpdated;
        private bool _physicsUpdated;
	
        private Camera _cam;

        private Vector2 _heightMinMax;

        // Game mode data 
        private int _activeLODIndex = -1;
        private MeshFilter _terrainMeshFilter;
        private Material _terrainMatInstance;

        private static Dictionary<int, SphereMesh> _sphereGenerators;

        private void Start()
        {
	        _cam = GameManager.Instance.GetMainCamera();
	        
            if (GameManager.Instance.gameMode == GameManager.GameMode.Explore)
            {
	            // Todo : are the setting ready? 
                HandleExploreModeGeneration();
            }
            else if (GameManager.Instance.gameMode == GameManager.GameMode.Editing)
            {
	            Debug.Log("initial update");
                _shapeUpdated = true;
                _shadingUpdated = true;
                _physicsUpdated = true;
                HandleEditModeGeneration();
            }
        }
        

        private void HandleExploreModeGeneration()
        {
	        // Generate LOD meshes
	        _lodMeshes = new Mesh[ResolutionSettings.NumLODLevels];
	        for (int i = 0; i < _lodMeshes.Length; i++) {
		        Vector2 lodTerrainHeightMinMax = GenerateTerrainMesh (ref _lodMeshes[i], resolutionSettings.GetLODResolution (i));
		        // Use min/max height of first (most detailed) LOD
		        if (i == 0) {
			        _heightMinMax = lodTerrainHeightMinMax;
		        }
	        }
	        
	        // // Generate collision mesh
	        // GenerateCollisionMesh (resolutionSettings.collider);
	        //
	        // // Create terrain renderer and set shading properties on the instanced material
	        // terrainMatInstance = new Material (body.shading.terrainMaterial);
	        // body.shading.Initialize (body.shape);
	        // body.shading.SetTerrainProperties (terrainMatInstance, heightMinMax, BodyScale);
	        // GameObject terrainHolder = GetOrCreateMeshObject ("Terrain Mesh", null, terrainMatInstance);
	        // terrainMeshFilter = terrainHolder.GetComponent<MeshFilter> ();
	        //
	        //
	        // // Add collider
	        // MeshCollider collider;
	        // if (!terrainHolder.TryGetComponent<MeshCollider> (out collider)) {
		       //  collider = terrainHolder.AddComponent<MeshCollider> ();
	        // }
	        //
	        // var collisionBakeTimer = System.Diagnostics.Stopwatch.StartNew ();
	        // MeshBaker.BakeMeshImmediate (collisionMesh);
	        // collider.sharedMesh = collisionMesh;
	        //
	        // ReleaseAllBuffers ();
        }

        private void HandleEditModeGeneration()
        {
            if (_shapeUpdated)
            {
	            Debug.Log("Shape and Shading Update");
                _shapeUpdated = false;
                _shadingUpdated = false;
                
                // _heightMinMax = GenerateTerrainMesh (ref _previewMesh, PickTerrainRes ());
            }
            // If only shading noise has changed, update it separately from shape to save time
            else if (_shadingUpdated)
            {
	            Debug.Log("Shading Update");
                _shadingUpdated = false;
                
                // ComputeHelper.CreateStructuredBuffer<Vector3> (ref _vertexBuffer, _previewMesh.vertices);
                // cBodySettings.shading.Initialize (cBodySettings.shape); 
                // Vector4[] shadingData = cBodySettings.shading.GenerateShadingData (_vertexBuffer);
                // _previewMesh.SetUVs (0, shadingData);
                
            }
            if (_physicsUpdated)
            {
	            Debug.Log("Physics Update");
                _physicsUpdated = false;
                
                // GeneratePhysics();
            }
        
            if (cBodySettings.shading != null)
            {
                // // Set material properties
                // cBodySettings.shading.Initialize (cBodySettings.shape);
                // cBodySettings.shading.SetTerrainProperties (cBodySettings.shading.terrainMaterial, _heightMinMax, cBodySettings.physics.GetSettings().radius);
            }
            
            //ReleaseAllBuffers();
        }
        
	    // Generates terrain mesh based on heights generated by the Shape object
		// Shading data from the Shading object is stored in the mesh uvs
		// Returns the min/max height of the terrain
		private Vector2 GenerateTerrainMesh (ref Mesh mesh, int resolution) {
			var (vertices, triangles) = CreateSphereVertsAndTris (resolution);
			ComputeHelper.CreateStructuredBuffer<Vector3> (ref _vertexBuffer, vertices);

			float edgeLength = (vertices[triangles[0]] - vertices[triangles[1]]).magnitude;

			// Set heights
			float[] heights = cBodySettings.shape.CalculateHeights (_vertexBuffer);

			Shape.ShapeSettings ss = cBodySettings.shape.GetSettings();
			// Perturb vertices to give terrain a less perfectly smooth appearance
			if (ss.perturbVertices && cBodySettings.shape.perturbCompute) {
				ComputeShader perturbShader = cBodySettings.shape.perturbCompute;
				float maxPerturbStrength = ss.perturbStrength * edgeLength / 2;

				perturbShader.SetBuffer (0, "points", _vertexBuffer);
				perturbShader.SetInt ("numPoints", vertices.Length);
				perturbShader.SetFloat ("maxStrength", maxPerturbStrength);

				ComputeHelper.Run (perturbShader, vertices.Length);
				Vector3[] pertData = new Vector3[vertices.Length];
				_vertexBuffer.GetData (vertices);
			}

			// Calculate terrain min/max height and set heights of vertices
			float minHeight = float.PositiveInfinity;
			float maxHeight = float.NegativeInfinity;
			for (int i = 0; i < heights.Length; i++) {
				float height = heights[i];
				vertices[i] *= height;
				minHeight = Mathf.Min (minHeight, height);
				maxHeight = Mathf.Max (maxHeight, height);
			}

			// Create mesh
			CreateMesh (ref mesh, vertices.Length);
			mesh.SetVertices (vertices);
			mesh.SetTriangles (triangles, 0, true);
			mesh.RecalculateNormals (); //

			// Shading noise data
			cBodySettings.shading.Initialize (cBodySettings.shape);
			Vector4[] shadingData = cBodySettings.shading.GenerateShadingData (_vertexBuffer);
			mesh.SetUVs (0, shadingData);

			// Create crude tangents (vectors perpendicular to surface normal)
			// This is needed (even though normal mapping is being done with triplanar)
			// because surfaceshader wants normals in tangent space
			var normals = mesh.normals;
			var crudeTangents = new Vector4[mesh.vertices.Length];
			for (int i = 0; i < vertices.Length; i++) {
				Vector3 normal = normals[i];
				crudeTangents[i] = new Vector4 (-normal.z, 0, normal.x, 1);
			}
			mesh.SetTangents (crudeTangents);

			return new Vector2 (minHeight, maxHeight);
		}
		
		// Generate sphere (or reuse if already generated) and return a copy of the vertices and triangles
		(Vector3[] vertices, int[] triangles) CreateSphereVertsAndTris (int resolution) {
			if (_sphereGenerators == null) {
				_sphereGenerators = new Dictionary<int, SphereMesh> ();
			}

			if (!_sphereGenerators.ContainsKey (resolution)) {
				_sphereGenerators.Add (resolution, new SphereMesh (resolution));
			}

			var generator = _sphereGenerators[resolution];

			var vertices = new Vector3[generator.Vertices.Length];
			var triangles = new int[generator.Triangles.Length];
			System.Array.Copy (generator.Vertices, vertices, vertices.Length);
			System.Array.Copy (generator.Triangles, triangles, triangles.Length);
			return (vertices, triangles);
		}
		
		void CreateMesh (ref Mesh mesh, int numVertices) {
			const int vertexLimit16Bit = 1 << 16 - 1; // 65535
			if (mesh == null) {
				mesh = new Mesh ();
			} else {
				mesh.Clear ();
			}
			mesh.indexFormat = (numVertices < vertexLimit16Bit) ? UnityEngine.Rendering.IndexFormat.UInt16 : UnityEngine.Rendering.IndexFormat.UInt32;
		}

        // public void GenerateMesh()
        // {
        //     // TODO
        //     Shape md = _cBodyData.shape;
        //     _meshObject = _meshGenerator.GenerateMesh(md.resolution, transform, gameObject);
        // }
        //
        // public void GenerateMaterial()
        // {
        //     // TODO
        //     Shading md = _cBodyData.shading;
        //     _meshGenerator.Material.color = md.color;
        // }

        private void GeneratePhysics()
        {
            // TODO update physics of current mesh!
            Physics.PhysicsSettings pd = cBodySettings.physics.GetSettings();
            Transform tr = transform;
            tr.position = pd.initialPosition;
            tr.localScale = Vector3.one * pd.radius;

            cBody.mass = pd.surfaceGravity * pd.radius * pd.radius / Constants.GravitationalConstant;
        }
        
        public void OnShapeUpdate()
        {
            _shapeUpdated = true;
            // todo : check if it is better to check in the update instead of direct call
            HandleEditModeGeneration();
        }

        public void OnShadingUpdate()
        {
            _shadingUpdated = true;
            HandleEditModeGeneration();
        }
        
        public void OnPhysicsUpdate()
        {
            _physicsUpdated = true; 
            HandleEditModeGeneration();
        }

        public int PickTerrainRes()
        {
	        Debug.Log(this);
	        Debug.Log(previewMode);
	        Debug.Log(resolutionSettings);
	        return previewMode switch
            {
                PreviewMode.LOD0 => resolutionSettings.lod0,
                PreviewMode.LOD1 => resolutionSettings.lod1,
                PreviewMode.LOD2 => resolutionSettings.lod2,
                PreviewMode.CollisionRes => resolutionSettings.collider,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void ReleaseAllBuffers () {
            ComputeHelper.Release (_vertexBuffer);
            cBodySettings.shape?.ReleaseBuffers ();
            cBodySettings.shading?.ReleaseBuffers ();
        }
        
        [System.Serializable]
        public class ResolutionSettings {

            public const int NumLODLevels = 3;
            const int MAXAllowedResolution = 500;

            public int lod0 = 300;
            public int lod1 = 100;
            public int lod2 = 50;
            public int collider = 100;

            public int GetLODResolution (int lodLevel)
            {
                return lodLevel switch
                {
                    0 => lod0,
                    1 => lod1,
                    2 => lod2,
                    _ => lod2
                };
            }

            public void ClampResolutions () {
                lod0 = Mathf.Min (MAXAllowedResolution, lod0);
                lod1 = Mathf.Min (MAXAllowedResolution, lod1);
                lod2 = Mathf.Min (MAXAllowedResolution, lod2);
                collider = Mathf.Min (MAXAllowedResolution, collider);
            }
        }

    }
}