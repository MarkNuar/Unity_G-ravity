using System;
using CBodies.Settings.Effects;
using CBodies.Settings.Shape;
using JetBrains.Annotations;
using JsonSubTypes;
using Newtonsoft.Json;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable][CreateAssetMenu]
    public abstract class Shading : ScriptableObject
    {
        // OBSERVER
        [CanBeNull] protected CBodyGenerator Observer;

        public ComputeShader shadingDataCompute;
        public Material terrainMaterial = null;
        
        protected Vector4[] CachedShadingData;
        private ComputeBuffer _shadingBuffer;
        
        public OceanSettings oceanSettings;

        private static System.Random _prng = new System.Random ();
        
        public virtual void ReleaseBuffers () {
            ComputeHelper.Release (_shadingBuffer);
        }

        public virtual void Initialize (Shape.Shape shape) { }
        
        // Set shading properties on terrain
        public virtual void SetTerrainProperties (Material material, Vector2 heightMinMax, float bodyScale) 
        {

        }

        public virtual void SetOceanProperties(Material oceanMaterial)
        {
            
        }
        
        // Generate Vector4[] of shading data. This is stored in mesh uvs and used to help shade the body
        public Vector4[] GenerateShadingData (ComputeBuffer vertexBuffer) {
            int numVertices = vertexBuffer.count;
            Vector4[] shadingData = new Vector4[numVertices];

            if (shadingDataCompute) {
                // Set data
                SetShadingDataComputeProperties ();

                shadingDataCompute.SetInt ("numVertices", numVertices);
                shadingDataCompute.SetBuffer (0, "vertices", vertexBuffer);
                ComputeHelper.CreateAndSetBuffer<Vector4> (ref _shadingBuffer, numVertices, shadingDataCompute, "shadingData");

                // Run
                ComputeHelper.Run (shadingDataCompute, numVertices);

                // Get data
                _shadingBuffer.GetData (shadingData);
            }

            CachedShadingData = shadingData;
            return shadingData;
        }
        
        // Override this to set properties on the shadingDataCompute before it is run
        protected virtual void SetShadingDataComputeProperties () {

        }
        
        // OBSERVER PATTERN
        public void Subscribe(CBodyGenerator observer)
        {
            Observer = observer;
        }
        
        public void Unsubscribe()
        {
            Observer = null;
        }
        
        // MEMENTO PATTERN
        public abstract void InitSettings();
        public abstract ShadingSettings GetSettings();

        public abstract void SetSettings(ShadingSettings ss);

        [Serializable]
        public abstract class ShadingSettings
        {
            public bool randomize;
            public int seed = _prng.Next(-10000, 10000);
            
            public bool hasOcean;
            [Range (0, 1)]
            public float oceanLevel = 0.55f;
            
            

            //public Color color;
        }
    }
}
