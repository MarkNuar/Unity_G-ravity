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
        [CanBeNull] private CBodyGenerator _observer;

        // Mesh resolution
        [SerializeField] private int currentResolution;
        // Max height of the mountains of the CBody
        [SerializeField] private float currentMountainsHeight;
        [SerializeField] private bool currentPerturbVertices;
        [SerializeField] [Range (0, 1)] private float currentPerturbStrength = 0.7f;
        
        [NonSerialized] public ComputeShader perturbCompute;
        [NonSerialized] public ComputeShader heightMapCompute;
        private ComputeBuffer heightBuffer;
        
        public int resolution
        {
            get => currentResolution;
            set
            {
                if(currentResolution == value)
                    return;
                currentResolution = value;
                if(_observer)
                    _observer.OnShapeUpdate();
            }
        }
        
        public float mountainsHeight
        {
            get => currentMountainsHeight;
            set
            {
                if(Math.Abs(currentMountainsHeight - value) < Mathf.Epsilon)
                    return;
                currentMountainsHeight = value;
                if(_observer) _observer.OnShapeUpdate();
            } 
        }
        
        public float perturbStrength
        {
            get => currentPerturbStrength;
            set
            {
                if(Math.Abs(currentPerturbStrength - value) < Mathf.Epsilon)
                    return;
                currentPerturbStrength = value;
                if(_observer) _observer.OnShapeUpdate();
            } 
        }
        
        public bool perturbVertices
        {
            get => currentPerturbVertices;
            set
            {
                if(currentPerturbVertices == value)
                    return;
                currentPerturbVertices = value;
                if(_observer)
                    _observer.OnShapeUpdate();
            }
        }
    
        
        public void Init(int res)
        {
            currentResolution = res;

            mountainsHeight = Random.Range(ParameterValues.minMountainsHeight, ParameterValues.maxMountainsHeight);

            perturbStrength = 0.7f;
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

        public virtual float[] CalculateHeights (ComputeBuffer vertexBuffer) {
            //Debug.Log (System.Environment.StackTrace);
            // Set data
            SetShapeData ();
            heightMapCompute.SetInt ("numVertices", vertexBuffer.count);
            heightMapCompute.SetBuffer (0, "vertices", vertexBuffer);
            ComputeHelper.CreateAndSetBuffer<float> (ref heightBuffer, vertexBuffer.count, heightMapCompute, "heights");

            // Run
            ComputeHelper.Run (heightMapCompute, vertexBuffer.count);

            // Get heights
            float[] heights = new float[vertexBuffer.count];
            heightBuffer.GetData (heights);
            return heights;
        }

        public virtual void ReleaseBuffers () {
            ComputeHelper.Release (heightBuffer);
        }

        protected virtual void SetShapeData () {

        }
    }
}
