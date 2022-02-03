using System;
using JetBrains.Annotations;
using UnityEngine;
using Utilities;


namespace CBodies.Settings.Shape
{
    [Serializable][CreateAssetMenu]
    public abstract class Shape : ScriptableObject
    {
        // OBSERVER
        [CanBeNull] protected CBodyGenerator Observer;
        
        public ComputeShader perturbCompute;
        public ComputeShader heightMapCompute;
        
        private ComputeBuffer _heightBuffer;
        
        
        private static System.Random _prng = new System.Random ();
        
        
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
        public abstract void InitSettings();

        public abstract ShapeSettings GetSettings();

        public abstract void SetSettings(ShapeSettings ss);

        [Serializable]
        public abstract class ShapeSettings
        {
            public bool randomize = false;
            public int seed = 0;
            public bool perturbVertices = false;
            [Range (0, 1)] public float perturbStrength = 0.36f;

            public void RandomizeShape(bool rand)
            {
                randomize = rand;
                seed = rand ? _prng.Next(-10000, 10000) : 0;
            }
        }
    }
}
