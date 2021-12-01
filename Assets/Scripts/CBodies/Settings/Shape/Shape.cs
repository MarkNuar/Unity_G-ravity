using System;
using JetBrains.Annotations;
using UI.Menu.SystemEditing;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;


namespace CBodies.Settings.Shape
{
    [Serializable][CreateAssetMenu]
    public class Shape : ScriptableObject
    {
        // OBSERVER
        [CanBeNull] private CBodyGenerator _observer;
        // MEMENTO
        private ShapeSettings _shapeSettings;
        
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
            _observer = observer;
        }
            
        public void Unsubscribe()
        {
            _observer = null;
        }
        
        // MEMENTO PATTERN
        public ShapeSettings GetSettings()
        {
            return _shapeSettings;
        }

        public void SetSettings(ShapeSettings ss)
        {
            _shapeSettings = ss;
            if(_observer)
                _observer.OnShapeUpdate();
        }
        
        public void RandomInitialize(int res)
        {
            _shapeSettings = new ShapeSettings
            {
                resolution = res,
                mountainsHeight = Random.Range(ParameterValues.minMountainsHeight, ParameterValues.maxMountainsHeight),
                perturbStrength = 0.7f
                // ...
            };
            if(_observer)
                _observer.OnShapeUpdate();
        }

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
