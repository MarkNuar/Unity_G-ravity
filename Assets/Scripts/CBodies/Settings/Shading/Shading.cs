using System;
using JetBrains.Annotations;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable]
    public class Shading
    {
        [CanBeNull] private CBodyGenerator _observer;

        [SerializeField] private Color currentColor;
        
        
        // TODO set from somewhere!!!!
        [NonSerialized] public ComputeShader shadingDataCompute;
        [NonSerialized] public Material terrainMaterial = null;
        
        protected Vector4[] cachedShadingData;
        private ComputeBuffer shadingBuffer;

        public Color color
        {
            get => currentColor;
            set
            {
                if(currentColor == value)
                    return;
                currentColor = value;

                if(_observer)
                    _observer.OnShadingUpdate();
            }
        }
        

        public void Init()
        {
            currentColor = Random.ColorHSV();
            // ...
        }

        public void Subscribe(CBodyGenerator observer)
        {
            _observer = observer;
        }
        
        public void Unsubscribe()
        {
            _observer = null;
        }
        
        public virtual void ReleaseBuffers () {
            ComputeHelper.Release (shadingBuffer);
        }
        
        // 
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
                ComputeHelper.CreateAndSetBuffer<Vector4> (ref shadingBuffer, numVertices, shadingDataCompute, "shadingData");

                // Run
                ComputeHelper.Run (shadingDataCompute, numVertices);

                // Get data
                shadingBuffer.GetData (shadingData);
            }

            cachedShadingData = shadingData;
            return shadingData;
        }
        
        // Override this to set properties on the shadingDataCompute before it is run
        protected virtual void SetShadingDataComputeProperties () {

        }
    }
}
