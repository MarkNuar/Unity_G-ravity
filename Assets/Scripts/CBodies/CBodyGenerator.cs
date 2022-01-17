﻿using System;
using System.Collections.Generic;
using CBodies.Settings;
using CBodies.Settings.Shape;
using UnityEditor;
using UnityEngine;
using Utilities;
using Physics = CBodies.Settings.Physics.Physics;

namespace CBodies
{
	public class CBodyGenerator : MonoBehaviour
	{
		public CBody cBody;

		public enum PreviewMode
		{
			LOD0,
			LOD1,
			LOD2,
			CollisionRes
		}

		public ResolutionSettings resolutionSettings = new ResolutionSettings();
		public PreviewMode previewMode = PreviewMode.LOD1;

		// CBodySettings
		public CBodySettings cBodySettings;

		// Meshes
		private GameObject _terrainHolder = null;
		private GameObject _colliderHolder = null;
		
		
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
			// todo
			// if game mode = exploration
			// instantiate a copy of the material for the newly created cbody
		}
		
		private void Update()
		{
			if (!_terrainHolder) return;
			if(GameManager.Instance.gameMode != GameManager.GameMode.Editing) return;
			Transform transform1 = _terrainHolder.transform;
			transform1.RotateAround(transform1.position, transform1.up, 
				cBodySettings.physics.GetSettings().rotationSpeed*Time.deltaTime);

			// if (Input.GetButtonDown("Fire1"))
			// {
			// 	Debug.LogError("Storing mesh");
			// 	AssetDatabase.CreateAsset(_previewMesh, "Assets/Art/Meshes/CBodyMesh.asset");
			// }
		}

		private void HandleExploreModeGeneration()
		{
			// Generate LOD meshes
			_lodMeshes = new Mesh[ResolutionSettings.NumLODLevels];
			for (int i = 0; i < _lodMeshes.Length; i++)
			{
				Vector2 lodTerrainHeightMinMax =
					GenerateShapeAndShading(ref _lodMeshes[i], resolutionSettings.GetLODResolution(i));
				// Use min/max height of first (most detailed) LOD
				if (i == 0)
				{
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

				_heightMinMax = GenerateShapeAndShading(ref _previewMesh, PickTerrainRes());

				// todo : the material is shared between cbodies, which is wrong
				var terrainMatInstance = new Material (cBodySettings.shading.terrainMaterial);
				cBody.surfaceMaterial = terrainMatInstance;
				_terrainHolder = GetOrCreateMeshObject("Terrain Mesh", _previewMesh, terrainMatInstance);
			}
			// If only shading noise has changed, update it separately from shape to save time
			else if (_shadingUpdated)
			{
				Debug.Log("Shading Update");
				_shadingUpdated = false;

				GenerateShading(_previewMesh);
			}

			if (_physicsUpdated)
			{
				Debug.Log("Physics Update");
				_physicsUpdated = false;

				GeneratePhysics();
			}

			if (cBodySettings.shading != null && cBody.surfaceMaterial != null)
			{
				// Set material properties
				cBodySettings.shading.Initialize(cBodySettings.shape);
				cBodySettings.shading.SetTerrainProperties(cBody.surfaceMaterial, _heightMinMax,
					BodyScale, cBodySettings.ocean.GetSettings().GetOceanLevel());
			}

			ReleaseAllBuffers();
		}

		// Generates terrain mesh based on heights generated by the Shape object
		// Shading data from the Shading object is stored in the mesh uvs
		// Returns the min/max height of the terrain
		private Vector2 GenerateShapeAndShading(ref Mesh mesh, int resolution)
		{
			var (vertices, triangles) = CreateSphereVertsAndTris(resolution);
			ComputeHelper.CreateStructuredBuffer<Vector3>(ref _vertexBuffer, vertices);

			float edgeLength = (vertices[triangles[0]] - vertices[triangles[1]]).magnitude;

			// Set heights
			float[] heights = cBodySettings.shape.CalculateHeights(_vertexBuffer);

			Shape.ShapeSettings ss = cBodySettings.shape.GetSettings();
			// Perturb vertices to give terrain a less perfectly smooth appearance
			if (ss.perturbVertices && cBodySettings.shape.perturbCompute)
			{
				ComputeShader perturbShader = cBodySettings.shape.perturbCompute;
				float maxPerturbStrength = ss.perturbStrength * edgeLength / 2;

				perturbShader.SetBuffer(0, "points", _vertexBuffer);
				perturbShader.SetInt("numPoints", vertices.Length);
				perturbShader.SetFloat("maxStrength", maxPerturbStrength);

				ComputeHelper.Run(perturbShader, vertices.Length);
				Vector3[] pertData = new Vector3[vertices.Length];
				_vertexBuffer.GetData(vertices);
			}

			// Calculate terrain min/max height and set heights of vertices
			float minHeight = float.PositiveInfinity;
			float maxHeight = float.NegativeInfinity;
			for (int i = 0; i < heights.Length; i++)
			{
				float height = heights[i];
				vertices[i] *= height;
				minHeight = Mathf.Min(minHeight, height);
				maxHeight = Mathf.Max(maxHeight, height);
			}
			
			_heightMinMax = new Vector2(minHeight, maxHeight);

			// Create mesh
			CreateMesh(ref mesh, vertices.Length);
			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0, true);
			mesh.RecalculateNormals();

			//todo shading
			// Shading noise data
			cBodySettings.shading.Initialize (cBodySettings.shape);
			Vector4[] shadingData = cBodySettings.shading.GenerateShadingData (_vertexBuffer);
			mesh.SetUVs (0, shadingData);
			
			// Create crude tangents (vectors perpendicular to surface normal)
			// This is needed (even though normal mapping is being done with tri planar)
			// because surface shader wants normals in tangent space
			var normals = mesh.normals;
			var crudeTangents = new Vector4[mesh.vertices.Length];
			for (int i = 0; i < vertices.Length; i++) {
				Vector3 normal = normals[i];
				crudeTangents[i] = new Vector4 (-normal.z, 0, normal.x, 1);
			}
			// mesh.SetTangents (crudeTangents);

			return new Vector2(minHeight, maxHeight);
		}

		private void GenerateShading(Mesh mesh)
		{
			ComputeHelper.CreateStructuredBuffer<Vector3>(ref _vertexBuffer, mesh.vertices);
			cBodySettings.shading.Initialize(cBodySettings.shape);
			Vector4[] shadingData = cBodySettings.shading.GenerateShadingData(_vertexBuffer);
			mesh.SetUVs(0, shadingData);
		}

		// Generate sphere (or reuse if already generated) and return a copy of its vertices and triangles (vertex indices)
		(Vector3[] vertices, int[] triangles) CreateSphereVertsAndTris(int resolution)
		{
			// If not created, creates a dictionary that stores different meshes, for each resolution
			if (_sphereGenerators == null)
			{
				_sphereGenerators = new Dictionary<int, SphereMesh>();
			}

			if (!_sphereGenerators.ContainsKey(resolution))
			{
				_sphereGenerators.Add(resolution, new SphereMesh(resolution));
			}

			SphereMesh generator = _sphereGenerators[resolution];

			var vertices = new Vector3[generator.Vertices.Length];
			var triangles = new int[generator.Triangles.Length];
			Array.Copy(generator.Vertices, vertices, vertices.Length);
			Array.Copy(generator.Triangles, triangles, triangles.Length);
			return (vertices, triangles);
		}

		void CreateMesh(ref Mesh mesh, int numVertices)
		{
			const int vertexLimit16Bit = 1 << 16 - 1; // 65535
			if (mesh == null)
			{
				mesh = new Mesh();
			}
			else
			{
				mesh.Clear();
			}

			mesh.indexFormat = (numVertices < vertexLimit16Bit)
				? UnityEngine.Rendering.IndexFormat.UInt16
				: UnityEngine.Rendering.IndexFormat.UInt32;
		}

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

		public void OnInitialUpdate()
		{
			_shapeUpdated = true;
			_shadingUpdated = true;
			_physicsUpdated = true;
			
			switch (GameManager.Instance.gameMode)
			{
				case GameManager.GameMode.Editing:
					HandleEditModeGeneration();
					break;
				case GameManager.GameMode.Explore:
					HandleExploreModeGeneration();
					break;
				default:
					break;
			}
		}

		// Gets child object with specified name.
        // If it doesn't exist, then creates object with that name, adds mesh renderer/filter and attaches mesh and material
        GameObject GetOrCreateMeshObject (string name, Mesh mesh, Material material) {
	        // Find/create object
	        var child = transform.Find (name);
	        if (!child) {
		        child = new GameObject (name).transform;
		        child.parent = transform;
		        child.localPosition = Vector3.zero;
		        child.localRotation = Quaternion.identity;
		        child.localScale = Vector3.one;
		        child.gameObject.layer = gameObject.layer;
	        }

	        // Add mesh components
	        MeshFilter filter;
	        if (!child.TryGetComponent<MeshFilter> (out filter)) {
		        filter = child.gameObject.AddComponent<MeshFilter> ();
	        }
	        filter.sharedMesh = mesh;

	        MeshRenderer renderer;
	        if (!child.TryGetComponent<MeshRenderer> (out renderer)) {
		        renderer = child.gameObject.AddComponent<MeshRenderer> ();
	        }
	        renderer.sharedMaterial = material;

	        return child.gameObject;
        }
        
        // Radius of the ocean (0 if no ocean exists)
        public float GetOceanRadius () {
	        if (!cBodySettings.ocean.GetSettings().hasOcean) {
		        return 0;
	        }
	        return UnscaledOceanRadius * BodyScale;
        }

        private float UnscaledOceanRadius => Mathf.Lerp (_heightMinMax.x, 1, cBodySettings.ocean.GetSettings().GetOceanLevel());

        public float BodyScale => cBodySettings.physics.GetSettings().radius;

        private int PickTerrainRes()
        {
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

        private void OnDestroy()
        {
	        ReleaseAllBuffers();
        }

        [System.Serializable]
        public class ResolutionSettings {

            public const int NumLODLevels = 3;
            const int MAXAllowedResolution = 500;

            public int lod0 = 200;
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