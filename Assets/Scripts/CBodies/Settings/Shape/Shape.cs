using System;
using JetBrains.Annotations;
using JsonSubTypes;
using Newtonsoft.Json;
using UI.Menu.SystemEditing;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;


namespace CBodies.Settings.Shape
{
    [Serializable][CreateAssetMenu]
    public abstract class Shape : ScriptableObject
    {
        // OBSERVER
        [CanBeNull] protected CBodyGenerator Observer;
        
        // MEMENTO
        [SerializeReference] protected ShapeSettings shapeSettings;

        
        public ComputeShader perturbCompute;
        public ComputeShader heightMapCompute;
        
        private ComputeBuffer _heightBuffer;
        
        
        public virtual float[] CalculateHeights (ComputeBuffer vertexBuffer) {
            //Debug.Log (System.Environment.StackTrace);
            // Set data
            SetShapeData ();
            heightMapCompute.SetInt ("numVertices", vertexBuffer.count);
            heightMapCompute.SetBuffer (0, "vertices", vertexBuffer);
            ComputeHelper.CreateAndSetBuffer<float> (ref _heightBuffer, vertexBuffer.count, heightMapCompute, "heights");

            // Run
            ComputeHelper.Run (heightMapCompute, vertexBuffer.count);

            // Get heights
            float[] heights = new float[vertexBuffer.count];
            _heightBuffer.GetData (heights);
            return heights;
        }

        public virtual void ReleaseBuffers () {
            ComputeHelper.Release (_heightBuffer);
        }

        protected virtual void SetShapeData () {

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
        public ShapeSettings GetSettings()
        {
            return shapeSettings;
        }

        public void SetSettings (ShapeSettings ss)
        {
            shapeSettings = ss;
            if(Observer)
                Observer.OnShapeUpdate();
        }

        public abstract void RandomInitialize(int res);

        [Serializable]
        public class ShapeSettings
        {
            public bool randomize;
            
            public int seed;
            // Mesh resolution
            
            public int resolution;
            // Max height of the mountains of the CBody
            
            public float mountainsHeight;
            
            public bool perturbVertices;
            
            [Range (0, 1)] public float perturbStrength = 0.7f;
            
        }
    }
}
