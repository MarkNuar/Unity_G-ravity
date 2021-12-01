using System;
using JetBrains.Annotations;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable][CreateAssetMenu]
    public class Shading : ScriptableObject
    {
        // OBSERVER
        [CanBeNull] private CBodyGenerator _observer;
        // MEMENTO 
        private ShadingSettings _shadingSettings;
        
        public ComputeShader shadingDataCompute;
        public Material terrainMaterial = null;
        
        protected Vector4[] CachedShadingData;
        private ComputeBuffer _shadingBuffer;
        
        public virtual void ReleaseBuffers () {
            ComputeHelper.Release (_shadingBuffer);
        }
        
        public virtual void Initialize (Shape.Shape shape) { }
        
        // Set shading properties on terrain
        public virtual void SetTerrainProperties (Material material, Vector2 heightMinMax, float bodyScale) {

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
            _observer = observer;
        }
        
        public void Unsubscribe()
        {
            _observer = null;
        }

        // MEMENTO PATTERN
        public ShadingSettings GetSettings()
        {
            return _shadingSettings;
        }

        public void SetSettings(ShadingSettings ss)
        {
            _shadingSettings = ss;
            if(_observer)
                _observer.OnShadingUpdate();
        }
        
        public void RandomInitialize()
        {
            _shadingSettings = new ShadingSettings
            {
                color = Random.ColorHSV()
                // ...
            };
            if(_observer)
                _observer.OnShadingUpdate();
        }

        [Serializable]
        public class ShadingSettings
        {
            public bool randomize;
            public int seed;
            
            public Color color;
        }
    }
}
